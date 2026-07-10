namespace RoadRegistry.Infrastructure.MartenDb.Setup;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DbUp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using RoadRegistry.BackOffice;

// Applies the versioned SQL migrations in Migrations/ sequentially (EF-style), tracked in a schema_migrations journal
// table, guarded by a Postgres advisory lock so only one instance migrates at a time. This is the sole schema
// mechanism now that Marten runs with AutoCreate.None (no runtime schema analysis). Registered by the migration
// owner (the Projector) and can be moved to a dedicated migration job later.
public sealed class DatabaseMigrator : IHostedService
{
    private const long AdvisoryLockKey = 6_827_314_590_112_233L; // arbitrary constant unique to road-registry migrations

    private readonly IConfiguration _configuration;
    private readonly DatabaseMigrationsGate _gate;
    private readonly ILogger<DatabaseMigrator> _logger;

    public DatabaseMigrator(IConfiguration configuration, DatabaseMigrationsGate gate, ILogger<DatabaseMigrator> logger)
    {
        _configuration = configuration;
        _gate = gate;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
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

            // Schema is ready: release anything waiting on migrations (e.g. the Marten projection daemon).
            _gate.MarkCompleted();
        }
        finally
        {
            await using var unlockCommand = new NpgsqlCommand("SELECT pg_advisory_unlock(@key)", lockConnection);
            unlockCommand.Parameters.AddWithValue("key", AdvisoryLockKey);
            await unlockCommand.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

// Completion signal set once DatabaseMigrator has applied all migrations. Consumers await Completed to ensure the
// schema exists before they touch it (order-independent: it does not matter whether the consumer starts before or
// after the migrator's hosted-service StartAsync). If migrations fail the migrator throws and the host is torn down,
// so Completed simply never fires and no consumer proceeds.
public sealed class DatabaseMigrationsGate
{
    private readonly TaskCompletionSource _completed = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task Completed => _completed.Task;

    public void MarkCompleted() => _completed.TrySetResult();
}

public static class DatabaseMigratorExtensions
{
    public static IServiceCollection AddMartenDatabaseMigrations(this IServiceCollection services)
    {
        return services
            .AddSingleton<DatabaseMigrationsGate>()
            .AddHostedService<DatabaseMigrator>();
    }

    public static Task RunMartenDatabaseMigrationsAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        var migrator = serviceProvider.GetServices<IHostedService>().OfType<DatabaseMigrator>().Single();
        return migrator.StartAsync(cancellationToken);
    }
}
