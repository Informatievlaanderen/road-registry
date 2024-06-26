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
    public DbSet<RoadSegmentEuropeanRoadAttributeLatestItem> RoadSegmentEuropeanRoadAttributes => Set<RoadSegmentEuropeanRoadAttributeLatestItem>();
    public DbSet<RoadSegmentNationalRoadAttributeLatestItem> RoadSegmentNationalRoadAttributes => Set<RoadSegmentNationalRoadAttributeLatestItem>();
    public DbSet<RoadSegmentNumberedRoadAttributeLatestItem> RoadSegmentNumberedRoadAttributes => Set<RoadSegmentNumberedRoadAttributeLatestItem>();
    public DbSet<RoadSegmentLaneAttributeLatestItem> RoadSegmentLaneAttributes => Set<RoadSegmentLaneAttributeLatestItem>();
    public DbSet<RoadSegmentSurfaceAttributeLatestItem> RoadSegmentSurfaceAttributes => Set<RoadSegmentSurfaceAttributeLatestItem>();
    public DbSet<RoadSegmentWidthAttributeLatestItem> RoadSegmentWidthAttributes => Set<RoadSegmentWidthAttributeLatestItem>();
    public DbSet<GradeSeparatedJunctionLatestItem> GradeSeparatedJunctions => Set<GradeSeparatedJunctionLatestItem>();
    
    public DbSet<Organizations.Version.OrganizationVersion> OrganizationVersions => Set<Organizations.Version.OrganizationVersion>();
    public DbSet<RoadNodes.Version.RoadNodeVersion> RoadNodeVersions => Set<RoadNodes.Version.RoadNodeVersion>();
    public DbSet<RoadSegments.Version.RoadSegmentVersion> RoadSegmentVersions => Set<RoadSegments.Version.RoadSegmentVersion>();
    public DbSet<RoadSegments.Version.RoadSegmentLaneAttributeVersion> RoadSegmentLaneVersions => Set<RoadSegments.Version.RoadSegmentLaneAttributeVersion>();
    public DbSet<RoadSegments.Version.RoadSegmentSurfaceAttributeVersion> RoadSegmentSurfaceVersions => Set<RoadSegments.Version.RoadSegmentSurfaceAttributeVersion>();
    public DbSet<RoadSegments.Version.RoadSegmentWidthAttributeVersion> RoadSegmentWidthVersions => Set<RoadSegments.Version.RoadSegmentWidthAttributeVersion>();
    public DbSet<RoadSegments.Version.RoadSegmentEuropeanRoadAttributeVersion> RoadSegmentEuropeanRoadVersions => Set<RoadSegments.Version.RoadSegmentEuropeanRoadAttributeVersion>();
    public DbSet<RoadSegments.Version.RoadSegmentNationalRoadAttributeVersion> RoadSegmentNationalRoadVersions => Set<RoadSegments.Version.RoadSegmentNationalRoadAttributeVersion>();
    public DbSet<RoadSegments.Version.RoadSegmentNumberedRoadAttributeVersion> RoadSegmentNumberedRoadVersions => Set<RoadSegments.Version.RoadSegmentNumberedRoadAttributeVersion>();
    public DbSet<GradeSeparatedJunctions.Version.GradeSeparatedJunctionVersion> GradeSeparatedJunctionVersions => Set<GradeSeparatedJunctions.Version.GradeSeparatedJunctionVersion>();

    // This needs to be here to please EF
    public IntegrationContext()
    { }

    // This needs to be DbContextOptions<T> for Autofac!
    public IntegrationContext(DbContextOptions<IntegrationContext> options)
        : base(options)
    { }
}
