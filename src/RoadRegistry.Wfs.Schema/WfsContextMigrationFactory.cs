namespace RoadRegistry.Wfs.Schema;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Hosts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

public class WfsContextMigrationFactory : RunnerDbContextMigrationFactory<WfsContext>
{
    public WfsContextMigrationFactory() :
        base(WellknownConnectionNames.WfsProjectionsAdmin, HistoryConfiguration)
    {
    }

    protected override void ConfigureSqlServerOptions(SqlServerDbContextOptionsBuilder sqlServerOptions)
    {
        sqlServerOptions.UseNetTopologySuite();
        base.ConfigureSqlServerOptions(sqlServerOptions);
    }

    protected override WfsContext CreateContext(DbContextOptions<WfsContext> migrationContextOptions)
    {
        return new WfsContext(migrationContextOptions);
    }

    private static MigrationHistoryConfiguration HistoryConfiguration =>
        new()
        {
            Schema = WellknownSchemas.WfsSchema,
            Table = MigrationTables.Wfs
        };
}