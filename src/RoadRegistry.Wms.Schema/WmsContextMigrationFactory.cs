namespace RoadRegistry.Wms.Schema;

using BackOffice;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

public class WmsContextMigrationFactory : RunnerDbContextMigrationFactory<WmsContext>
{
    public WmsContextMigrationFactory() :
        base(WellKnownConnectionNames.WmsProjectionsAdmin, HistoryConfiguration)
    {
    }

    private static MigrationHistoryConfiguration HistoryConfiguration =>
        new()
        {
            Schema = WellKnownSchemas.WmsSchema,
            Table = MigrationTables.Wms
        };

    protected override void ConfigureSqlServerOptions(SqlServerDbContextOptionsBuilder sqlServerOptions)
    {
        sqlServerOptions.UseNetTopologySuite();
        base.ConfigureSqlServerOptions(sqlServerOptions);
    }

    protected override WmsContext CreateContext(DbContextOptions<WmsContext> migrationContextOptions)
    {
        return new WmsContext(migrationContextOptions);
    }
}
