namespace RoadRegistry.WmsWfsV2.Schema;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

public class WmsWfsV2ContextMigratorFactory : DbContextMigratorFactory<WmsWfsV2Context>
{
    public static readonly MigrationHistoryConfiguration Configuration = new()
    {
        Schema = WellKnownSchemas.WmsWfsV2Schema,
        Table = MigrationTables.WmsWfsV2
    };

    public WmsWfsV2ContextMigratorFactory()
        : base(WellKnownConnectionNames.WmsWfsV2ProjectionsAdmin, Configuration)
    {
    }

    protected override void ConfigureSqlServerOptions(SqlServerDbContextOptionsBuilder sqlServerOptions)
    {
        WmsWfsV2Context.ConfigureSqlServerOptions(sqlServerOptions);
    }

    protected override WmsWfsV2Context CreateContext(DbContextOptions<WmsWfsV2Context> migrationContextOptions)
    {
        return new WmsWfsV2Context(migrationContextOptions);
    }
}
