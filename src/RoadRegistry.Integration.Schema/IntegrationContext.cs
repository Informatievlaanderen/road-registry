namespace RoadRegistry.Integration.Schema;

using System;
using BackOffice;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Microsoft.EntityFrameworkCore;
using RoadSegments;

public class IntegrationContext : RunnerDbContext<IntegrationContext>
{
    public IntegrationContext()
    {
    }

    // This needs to be DbContextOptions<T> for Autofac!
    public IntegrationContext(DbContextOptions<IntegrationContext> options)
        : base(options)
    {
        if (!Database.IsInMemory())
        {
            Database.SetCommandTimeout(TimeSpan.FromMinutes(10));
        }
    }

    public override string ProjectionStateSchema => WellKnownSchemas.IntegrationSchema;

    // public DbSet<OrganizationRecord> Organizations { get; set; }
    // public DbSet<RoadNodeRecord> RoadNodes { get; set; }
    public DbSet<RoadSegmentLatestItem> RoadSegments { get; set; }
    // public DbSet<RoadSegmentEuropeanRoadAttributeRecord> RoadSegmentEuropeanRoadAttributes { get; set; }
    // public DbSet<RoadSegmentNationalRoadAttributeRecord> RoadSegmentNationalRoadAttributes { get; set; }
    // public DbSet<RoadSegmentNumberedRoadAttributeRecord> RoadSegmentNumberedRoadAttributes { get; set; }
    // public DbSet<RoadSegmentLaneAttributeRecord> RoadSegmentLaneAttributes { get; set; }
    // public DbSet<RoadSegmentSurfaceAttributeRecord> RoadSegmentSurfaceAttributes { get; set; }
    // public DbSet<RoadSegmentWidthAttributeRecord> RoadSegmentWidthAttributes { get; set; }
    // public DbSet<GradeSeparatedJunctionRecord> GradeSeparatedJunctions { get; set; }

    protected override void OnConfiguringOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseRoadRegistryInMemorySqlServer();
    }
}
