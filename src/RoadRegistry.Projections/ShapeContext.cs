namespace RoadRegistry.Projections
{
    using Aiv.Vbr.ProjectionHandling.Runner;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;

    public class ShapeContext : RunnerDbContext<ShapeContext>
    {
        public override string ProjectionStateSchema => Schema.ProjectionMetaData;

        public DbSet<RoadNodeRecord> RoadNodes { get; set; }
        public DbSet<RoadSegmentRecord> RoadSegments { get; set; }
        public DbSet<RoadSegmentLaneAttributeRecord> RoadSegmentLaneAttributes { get; set; }
        public DbSet<RoadSegmentWidthAttributeRecord> RoadSegmentWidthAttributes { get; set; }
        public DbSet<RoadSegmentSurfaceAttributeRecord> RoadSegmentSurfaceAttributes { get; set; }
        public DbSet<RoadSegmentEuropeanRoadAttributeRecord> RoadSegmentEuropeanRoadAttributes { get; set; }
        public DbSet<RoadSegmentNationalRoadAttributeRecord> RoadSegmentNationalRoadAttributes { get; set; }
        public DbSet<RoadSegmentNumberedRoadAttributeRecord> RoadSegmentNumberedRoadAttributes { get; set; }
        public DbSet<GradeSeparatedJunctionRecord> GradeSeparatedJunctions { get; set; }
        public DbSet<OrganizationRecord> Organizations { get; set; }
        public DbSet<RoadNetworkInfo> RoadNetworkInfo { get; set; }

        public ShapeContext() {}

        // This needs to be DbContextOptions<T> for Autofac!
        public ShapeContext(DbContextOptions<ShapeContext> options)
            : base(options) { }

        protected override void OnConfiguringOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=EFProviders.InMemory.RoadRegistry.RoadRegistryContext;Trusted_Connection=True;");
    }
}
