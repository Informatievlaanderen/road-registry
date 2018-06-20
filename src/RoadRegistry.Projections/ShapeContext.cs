namespace RoadRegistry.Projections
{
    using Aiv.Vbr.ProjectionHandling.Runner;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;

    public class ShapeContext : RunnerDbContext<ShapeContext>
    {
        public override string ProjectionStateSchema => Schema.Oslo;

        public DbSet<RoadNodeRecord> RoadNodes { get; set; }
        public DbSet<RoadSegmentRecord> RoadSegments { get; set; }
        public DbSet<RoadReferencePointRecord> RoadReferencePoints { get; set; }
        public DbSet<RoadSegmentDynamicLaneAttributeRecord> RoadLaneAttrubutes { get; set; }
        public DbSet<RoadSegmentDynamicWidthAttributeRecord> RoadWidthAttrubutes { get; set; }
        public DbSet<RoadSegmentDynamicHardeningAttributeRecord> RoadHardeningAttrubutes { get; set; }
        public DbSet<RoadSegmentEuropeanRoadAttributeRecord> EuropeanRoadAttributes { get; set; }

        public ShapeContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public ShapeContext(DbContextOptions<ShapeContext> options)
            : base(options) { }

        protected override void OnConfiguringOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=EFProviders.InMemory.RoadRegistry.RoadRegistryContext;Trusted_Connection=True;");
    }
}
