namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Schema
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Hosts;
    using Microsoft.EntityFrameworkCore;

    public class ProducerSnapshotContext : RunnerDbContext<ProducerSnapshotContext>
    {
        public ProducerSnapshotContext()
        {
        }

        // This needs to be DbContextOptions<T> for Autofac!
        public ProducerSnapshotContext(DbContextOptions<ProducerSnapshotContext> options)
            : base(options)
        {
        }

        public override string ProjectionStateSchema => WellknownSchemas.ProducerSnapshotMetaSchema;
        public DbSet<RoadNodeRecord> RoadNodes { get; set; }

        protected override void OnConfiguringOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=EFProviders.InMemory.RoadRegistry.RoadRegistryContext;Trusted_Connection=True;");
        }
    }
}
