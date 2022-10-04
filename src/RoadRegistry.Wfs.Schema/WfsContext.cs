namespace RoadRegistry.Wfs.Schema
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Hosts;
    using Microsoft.EntityFrameworkCore;

    public class WfsContext : RunnerDbContext<WfsContext>
    {
        public override string ProjectionStateSchema => WellknownSchemas.WfsMetaSchema;
        public DbSet<RoadSegmentRecord> RoadSegments { get; set; }
        public DbSet<RoadNodeRecord> RoadNodes { get; set; }

        // Temporarily unsupported
        // public DbSet<GradeSeparatedJunctionRecord> GradeSeparatedJunctions { get; set; }
        public WfsContext() {}

        // This needs to be DbContextOptions<T> for Autofac!
        public WfsContext(DbContextOptions<WfsContext> options)
            : base(options) { }

        protected override void OnConfiguringOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=EFProviders.InMemory.RoadRegistry.RoadRegistryContext;Trusted_Connection=True;");
    }
}
