namespace RoadRegistry.Integration.Schema;

using BackOffice;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Microsoft.EntityFrameworkCore;
using RoadNodes;
using RoadSegments;

public class IntegrationContext : RunnerDbContext<IntegrationContext>
{
    public override string ProjectionStateSchema => WellKnownSchemas.IntegrationSchema;

    // public DbSet<OrganizationRecord> Organizations { get; set; }
    public DbSet<RoadNodeLatestItem> RoadNodes { get; set; }
    public DbSet<RoadSegmentLatestItem> RoadSegments  => Set<RoadSegmentLatestItem>();
    // public DbSet<RoadSegmentEuropeanRoadAttributeRecord> RoadSegmentEuropeanRoadAttributes { get; set; }
    // public DbSet<RoadSegmentNationalRoadAttributeRecord> RoadSegmentNationalRoadAttributes { get; set; }
    // public DbSet<RoadSegmentNumberedRoadAttributeRecord> RoadSegmentNumberedRoadAttributes { get; set; }
    // public DbSet<RoadSegmentLaneAttributeRecord> RoadSegmentLaneAttributes { get; set; }
    // public DbSet<RoadSegmentSurfaceAttributeRecord> RoadSegmentSurfaceAttributes { get; set; }
    // public DbSet<RoadSegmentWidthAttributeRecord> RoadSegmentWidthAttributes { get; set; }
    // public DbSet<GradeSeparatedJunctionRecord> GradeSeparatedJunctions { get; set; }

    // This needs to be here to please EF
    public IntegrationContext()
    { }

    // This needs to be DbContextOptions<T> for Autofac!
    public IntegrationContext(DbContextOptions<IntegrationContext> options)
        : base(options)
    { }
}
