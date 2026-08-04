namespace RoadRegistry.Infrastructure.MartenDb.Setup;

using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DbUp;
using DbUp.Engine.Output;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using RoadRegistry.BackOffice;

public sealed class DatabaseMigratorFactory : IDbMigratorFactory
{
    public IDbMigrator CreateMigrator(IConfiguration configuration, ILoggerFactory loggerFactory)
    {
        return new DatabaseMigrator(configuration, loggerFactory);
    }
}

// Applies the versioned SQL migrations in Migrations/ sequentially (EF-style), tracked in a schema_migrations journal
// table, guarded by a Postgres advisory lock so only one instance migrates at a time. This is the sole schema
// mechanism now that Marten runs with AutoCreate.None (no runtime schema analysis). Run from Program before the host
// starts (alongside the EF IDbMigrators), so the schema exists before any hosted service touches Marten.
public sealed class DatabaseMigrator : IDbMigrator
{
    private const long AdvisoryLockKey = 6_827_314_590_112_233L; // arbitrary constant unique to road-registry migrations

    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseMigrator> _logger;

    public DatabaseMigrator(IConfiguration configuration, ILoggerFactory loggerFactory)
    {
        _configuration = configuration;
        _logger = loggerFactory.CreateLogger<DatabaseMigrator>();
    }

    public async Task MigrateAsync(CancellationToken cancellationToken)
    {
        var connectionString = _configuration.GetRequiredConnectionString(WellKnownConnectionNames.Marten);

        // Serialize concurrent instances (e.g. horizontally scaled owner) so only one applies migrations at a time.
        await using var lockConnection = new NpgsqlConnection(connectionString);
        await lockConnection.OpenAsync(cancellationToken);

        await using (var lockCommand = new NpgsqlCommand("SELECT pg_advisory_lock(@key)", lockConnection))
        {
            lockCommand.Parameters.AddWithValue("key", AdvisoryLockKey);
            await lockCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        try
        {
            // The DbUp journal lives in the eventstore schema, and DbUp creates that journal table BEFORE
            // running any migration script — but the eventstore schema itself is only created inside the
            // baseline script. Pre-create it so the journal table can be provisioned on a fresh database.
            await using (var schemaCommand = new NpgsqlCommand($"CREATE SCHEMA IF NOT EXISTS \"{WellKnownSchemas.MartenEventStore}\"", lockConnection))
            {
                await schemaCommand.ExecuteNonQueryAsync(cancellationToken);
            }

            var upgrader = DeployChanges.To
                .PostgresqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(
                    typeof(DatabaseMigrator).Assembly,
                    name => name.Contains(".Migrations.") && name.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
                .WithTransactionPerScript()
                .WithVariablesDisabled()
                .JournalToPostgresqlTable(WellKnownSchemas.MartenEventStore, "schema_migrations")
                .LogTo(new DbUpLogger(_logger))
                .Build();

            var result = upgrader.PerformUpgrade();
            if (!result.Successful)
            {
                _logger.LogError(result.Error, "Database migration failed on script {Script}.", result.ErrorScript?.Name);
                throw result.Error;
            }

            _logger.LogInformation("Database migrations up to date ({Count} applied this run).", result.Scripts.Count());
        }
        finally
        {
            await using var unlockCommand = new NpgsqlCommand("SELECT pg_advisory_unlock(@key)", lockConnection);
            unlockCommand.Parameters.AddWithValue("key", AdvisoryLockKey);
            await unlockCommand.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    // DbUp's own ConsoleUpgradeLog runs every message through string.Format. A database error quoting one of these
    // migrations contains JSON braces, so formatting it throws FormatException from inside the logger - and because
    // that happens while DbUp is reporting the failure, the FormatException REPLACES the real error in
    // UpgradeResult.Error. The actual reason the script failed is then lost. Route DbUp's logging at our own logger
    // instead, format only when arguments were really supplied, and never let logging throw.
    private sealed class DbUpLogger : IUpgradeLog
    {
        private readonly ILogger _logger;

        public DbUpLogger(ILogger logger)
        {
            _logger = logger;
        }

        public void LogTrace(string format, params object[] args) => _logger.LogTrace("{DbUpMessage}", Render(format, args));
        public void LogDebug(string format, params object[] args) => _logger.LogDebug("{DbUpMessage}", Render(format, args));
        public void LogInformation(string format, params object[] args) => _logger.LogInformation("{DbUpMessage}", Render(format, args));
        public void LogWarning(string format, params object[] args) => _logger.LogWarning("{DbUpMessage}", Render(format, args));
        public void LogError(string format, params object[] args) => _logger.LogError("{DbUpMessage}", Render(format, args));
        public void LogError(Exception ex, string format, params object[] args) => _logger.LogError(ex, "{DbUpMessage}", Render(format, args));

        private static string Render(string format, object[]? args)
        {
            if (args is null || args.Length == 0)
            {
                return format;
            }

            try
            {
                return string.Format(CultureInfo.InvariantCulture, format, args);
            }
            catch (FormatException)
            {
                // The message is worth more than the substitution, and it must not replace the failure being reported.
                return format;
            }
        }
    }
}

public static class DatabaseMigratorExtensions
{
    public static IServiceCollection AddMartenDatabaseMigrator(this IServiceCollection services)
    {
        return services
            .AddSingleton<IDbMigrator, DatabaseMigrator>()
            .AddSingleton<IDbMigratorFactory, DatabaseMigratorFactory>()
            ;
    }

    // Convenience for callers (e.g. integration test setup) that want to apply the Marten schema imperatively without
    // going through the full IDbMigrator fan-out.
    public static Task RunMartenDatabaseMigrationsAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        var migrator = serviceProvider.GetServices<IDbMigrator>().OfType<DatabaseMigrator>().Single();
        return migrator.MigrateAsync(cancellationToken);
    }
}
