namespace RoadRegistry.WmsWfsV1Inwinning;

using BackOffice;
using Microsoft.EntityFrameworkCore;

public class WmsWfsV1InwinningContextMigratorFactory : DbContextMigratorFactory<WmsWfsV1InwinningContext>
{
    public static readonly MigrationHistoryConfiguration Configuration = new()
    {
        Schema = WellKnownSchemas.WmsWfsV1InwinningSchema,
        Table = MigrationTables.WmsWfsV1Inwinning
    };

    // The same database as the WmsWfsV2 read model, which is where the V1 WMS and WFS read models live as well.
    public WmsWfsV1InwinningContextMigratorFactory()
        : base(WellKnownConnectionNames.WmsWfsV2ProjectionsAdmin, Configuration)
    {
    }

    protected override WmsWfsV1InwinningContext CreateContext(DbContextOptions<WmsWfsV1InwinningContext> migrationContextOptions)
    {
        return new WmsWfsV1InwinningContext(migrationContextOptions);
    }
}
