namespace RoadRegistry.Producer.Snapshot.ProjectionHost.RoadNode
{
    using BackOffice;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates;
    using Microsoft.EntityFrameworkCore;

    public class RoadNodeProducerSnapshotContext : RunnerDbContext<RoadNodeProducerSnapshotContext>
    {
        public RoadNodeProducerSnapshotContext()
        {
        }

        // This needs to be DbContextOptions<T> for Autofac!
        public RoadNodeProducerSnapshotContext(DbContextOptions<RoadNodeProducerSnapshotContext> options)
            : base(options)
        {
        }

        public override string ProjectionStateSchema => WellknownSchemas.RoadNodeProducerSnapshotMetaSchema;
        public DbSet<RoadNodeRecord> RoadNodes { get; set; }

        protected override void OnConfiguringOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseRoadRegistryInMemorySqlServer();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //base.OnModelCreating(modelBuilder); DO NOT trigger base => Does assembly scan!
            modelBuilder.ApplyConfiguration(new ProjectionStatesConfiguration(ProjectionStateSchema));
            modelBuilder.ApplyConfiguration(new RoadNodeConfiguration());
        }
    }
}
