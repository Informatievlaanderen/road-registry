namespace RoadRegistry.Producer.Snapshot.ProjectionHost.RoadSegmentSurface
{
    using BackOffice;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates;
    using Microsoft.EntityFrameworkCore;

    public class RoadSegmentSurfaceProducerSnapshotContext : RunnerDbContext<RoadSegmentSurfaceProducerSnapshotContext>
    {
        public RoadSegmentSurfaceProducerSnapshotContext()
        {
        }

        // This needs to be DbContextOptions<T> for Autofac!
        public RoadSegmentSurfaceProducerSnapshotContext(DbContextOptions<RoadSegmentSurfaceProducerSnapshotContext> options)
            : base(options)
        {
        }

        public override string ProjectionStateSchema => WellKnownSchemas.RoadSegmentSurfaceProducerSnapshotMetaSchema;
        public DbSet<RoadSegmentSurfaceRecord> RoadSegmentSurfaces { get; set; }

        protected override void OnConfiguringOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseRoadRegistryInMemorySqlServer();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //base.OnModelCreating(modelBuilder); DO NOT trigger base => Does assembly scan!
            modelBuilder.ApplyConfiguration(new ProjectionStatesConfiguration(ProjectionStateSchema));
            modelBuilder.ApplyConfiguration(new RoadSegmentSurfaceConfiguration());
        }
    }
}
