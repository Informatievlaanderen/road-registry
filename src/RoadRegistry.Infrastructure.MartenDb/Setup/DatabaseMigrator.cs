namespace RoadRegistry.Infrastructure.MartenDb.Setup;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DbUp;
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
                .LogToConsole()
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
