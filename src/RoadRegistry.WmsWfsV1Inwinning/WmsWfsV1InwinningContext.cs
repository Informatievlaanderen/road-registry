namespace RoadRegistry.WmsWfsV1Inwinning;

using BackOffice;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Microsoft.EntityFrameworkCore;
using Records;

// What the V1 WMS and WFS no longer serve because it is ingewonnen, for as long as there is V1 data: the road segments
// and road nodes that are migrated to V2, and the V1 records that are marked IsV2 for it - which the V1 views filter on.
//
// It lives in the WMS/WFS database next to the V1 read models, but is a project of its own - RunnerDbContext applies
// every entity configuration in its context's assembly - so that it can be removed as a whole once there is no V1 data
// left: this project, its projection's registration in the Projector, and its tables.
public class WmsWfsV1InwinningContext : RunnerDbContext<WmsWfsV1InwinningContext>
{
    public WmsWfsV1InwinningContext()
    {
    }

    // This needs to be DbContextOptions<T> for Autofac!
    public WmsWfsV1InwinningContext(DbContextOptions<WmsWfsV1InwinningContext> options)
        : base(options)
    {
    }

    public override string ProjectionStateSchema => WellKnownSchemas.WmsWfsV1InwinningSchema;

    // Owned, and migrated, by this context.
    public DbSet<CompletedRoadSegmentRecord> CompletedRoadSegments { get; set; }
    public DbSet<CompletedRoadNodeRecord> CompletedRoadNodes { get; set; }

    // The V1 WMS and WFS tables - migrated by their own read models, only mapped here.
    public DbSet<V1WmsRoadSegmentRecord> V1WmsRoadSegments { get; set; }
    public DbSet<V1WmsEuropeanRoadRecord> V1WmsEuropeanRoads { get; set; }
    public DbSet<V1WmsNationalRoadRecord> V1WmsNationalRoads { get; set; }
    public DbSet<V1WfsRoadSegmentRecord> V1WfsRoadSegments { get; set; }
    public DbSet<V1WfsRoadNodeRecord> V1WfsRoadNodes { get; set; }

    protected override void OnConfiguringOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseRoadRegistryInMemorySqlServer();
    }
}
