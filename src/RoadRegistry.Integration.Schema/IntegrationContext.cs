namespace RoadRegistry.Integration.Schema;

using BackOffice;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using GradeSeparatedJunctions;
using Microsoft.EntityFrameworkCore;
using Organizations;
using RoadNodes;
using RoadSegments;

public class IntegrationContext : RunnerDbContext<IntegrationContext>
{
    public override string ProjectionStateSchema => WellKnownSchemas.IntegrationSchema;

    public DbSet<OrganizationLatestItem> Organizations => Set<OrganizationLatestItem>();
    public DbSet<RoadNodeLatestItem> RoadNodes => Set<RoadNodeLatestItem>();
    public DbSet<RoadSegmentLatestItem> RoadSegments => Set<RoadSegmentLatestItem>();
    public DbSet<RoadSegments.RoadSegmentVersion> RoadSegmentVersions => Set<RoadSegments.RoadSegmentVersion>();
    public DbSet<RoadSegmentEuropeanRoadAttributeLatestItem> RoadSegmentEuropeanRoadAttributes => Set<RoadSegmentEuropeanRoadAttributeLatestItem>();
    public DbSet<RoadSegmentNationalRoadAttributeLatestItem> RoadSegmentNationalRoadAttributes => Set<RoadSegmentNationalRoadAttributeLatestItem>();
    public DbSet<RoadSegmentNumberedRoadAttributeLatestItem> RoadSegmentNumberedRoadAttributes => Set<RoadSegmentNumberedRoadAttributeLatestItem>();
    public DbSet<RoadSegmentLaneAttributeLatestItem> RoadSegmentLaneAttributes => Set<RoadSegmentLaneAttributeLatestItem>();
    public DbSet<RoadSegmentSurfaceAttributeLatestItem> RoadSegmentSurfaceAttributes => Set<RoadSegmentSurfaceAttributeLatestItem>();
    public DbSet<RoadSegmentWidthAttributeLatestItem> RoadSegmentWidthAttributes => Set<RoadSegmentWidthAttributeLatestItem>();
    public DbSet<GradeSeparatedJunctionLatestItem> GradeSeparatedJunctions => Set<GradeSeparatedJunctionLatestItem>();

    // This needs to be here to please EF
    public IntegrationContext()
    { }

    // This needs to be DbContextOptions<T> for Autofac!
    public IntegrationContext(DbContextOptions<IntegrationContext> options)
        : base(options)
    { }
}
