namespace RoadRegistry.Integration.Schema;

using BackOffice;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

public class IntegrationContextMigrationFactory : SqlServerRunnerDbContextMigrationFactory<IntegrationContext>
{
    public IntegrationContextMigrationFactory()
        : base(WellKnownConnectionNames.EditorProjectionsAdmin, HistoryConfiguration)
    {
    }

    private static MigrationHistoryConfiguration HistoryConfiguration =>
        new()
        {
            Schema = WellKnownSchemas.EditorSchema,
            Table = MigrationTables.Editor
        };

    protected override void ConfigureSqlServerOptions(SqlServerDbContextOptionsBuilder sqlServerOptions)
    {
        sqlServerOptions.UseNetTopologySuite();
        base.ConfigureSqlServerOptions(sqlServerOptions);
    }

    protected override IntegrationContext CreateContext(DbContextOptions<IntegrationContext> migrationContextOptions)
    {
        return new IntegrationContext(migrationContextOptions);
    }
}
