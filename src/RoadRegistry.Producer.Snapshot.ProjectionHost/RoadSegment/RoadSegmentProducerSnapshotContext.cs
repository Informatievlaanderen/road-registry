namespace RoadRegistry.Producer.Snapshot.ProjectionHost.RoadSegment
{
    using BackOffice;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates;
    using Microsoft.EntityFrameworkCore;

    public class RoadSegmentProducerSnapshotContext : RunnerDbContext<RoadSegmentProducerSnapshotContext>
    {
        public RoadSegmentProducerSnapshotContext()
        {
        }

        // This needs to be DbContextOptions<T> for Autofac!
        public RoadSegmentProducerSnapshotContext(DbContextOptions<RoadSegmentProducerSnapshotContext> options)
            : base(options)
        {
        }

        public override string ProjectionStateSchema => WellknownSchemas.RoadSegmentProducerSnapshotMetaSchema;
        public DbSet<RoadSegmentRecord> RoadSegments { get; set; }

        protected override void OnConfiguringOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseRoadRegistryInMemorySqlServer();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //base.OnModelCreating(modelBuilder); DO NOT trigger base => Does assembly scan!
            modelBuilder.ApplyConfiguration(new ProjectionStatesConfiguration(ProjectionStateSchema));
            modelBuilder.ApplyConfiguration(new RoadSegmentConfiguration());
        }
    }
}
