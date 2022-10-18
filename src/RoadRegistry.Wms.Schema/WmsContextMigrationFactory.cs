namespace RoadRegistry.Wms.Schema;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Hosts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

public class WmsContextMigrationFactory : RunnerDbContextMigrationFactory<WmsContext>
{
    public WmsContextMigrationFactory() :
        base(WellknownConnectionNames.WmsProjectionsAdmin, HistoryConfiguration)
    {
    }

    protected override void ConfigureSqlServerOptions(SqlServerDbContextOptionsBuilder sqlServerOptions)
    {
        sqlServerOptions.UseNetTopologySuite();
        base.ConfigureSqlServerOptions(sqlServerOptions);
    }

    protected override WmsContext CreateContext(DbContextOptions<WmsContext> migrationContextOptions)
    {
        return new WmsContext(migrationContextOptions);
    }

    private static MigrationHistoryConfiguration HistoryConfiguration =>
        new()
        {
            Schema = WellknownSchemas.WmsSchema,
            Table = MigrationTables.Wms
        };
}
