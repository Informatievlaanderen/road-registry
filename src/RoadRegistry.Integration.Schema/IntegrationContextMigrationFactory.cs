namespace RoadRegistry.Integration.Schema;

using BackOffice;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.Npgsql;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

public class IntegrationContextMigrationFactory : NpgsqlRunnerDbContextMigrationFactory<IntegrationContext>
{
    public IntegrationContextMigrationFactory()
        : base(WellKnownConnectionNames.IntegrationProjectionsAdmin, HistoryConfiguration)
    {
    }

    private static MigrationHistoryConfiguration HistoryConfiguration =>
        new()
        {
            Table = MigrationTables.Integration,
            Schema = WellKnownSchemas.IntegrationSchema
        };

    protected override void ConfigureSqlServerOptions(NpgsqlDbContextOptionsBuilder sqlServerOptions)
    {
        sqlServerOptions.UseNetTopologySuite();
        base.ConfigureSqlServerOptions(sqlServerOptions);
    }

    protected override IntegrationContext CreateContext(
        DbContextOptions<IntegrationContext> migrationContextOptions)
    {
        return new IntegrationContext(migrationContextOptions);
    }
}
