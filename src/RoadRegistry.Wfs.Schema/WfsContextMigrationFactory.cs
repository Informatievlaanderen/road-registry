namespace RoadRegistry.Wfs.Schema;

using BackOffice;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

public class WfsContextMigrationFactory : SqlServerRunnerDbContextMigrationFactory<WfsContext>
{
    public WfsContextMigrationFactory() :
        base(WellKnownConnectionNames.WfsProjectionsAdmin, HistoryConfiguration)
    {
    }

    private static MigrationHistoryConfiguration HistoryConfiguration =>
        new()
        {
            Schema = WellKnownSchemas.WfsSchema,
            Table = MigrationTables.Wfs
        };

    protected override void ConfigureSqlServerOptions(SqlServerDbContextOptionsBuilder sqlServerOptions)
    {
        sqlServerOptions.UseNetTopologySuite();
        base.ConfigureSqlServerOptions(sqlServerOptions);
    }

    protected override WfsContext CreateContext(DbContextOptions<WfsContext> migrationContextOptions)
    {
        return new WfsContext(migrationContextOptions);
    }
}
