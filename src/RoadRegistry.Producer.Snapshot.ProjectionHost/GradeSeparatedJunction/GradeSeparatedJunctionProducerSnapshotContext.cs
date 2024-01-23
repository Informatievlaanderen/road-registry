namespace RoadRegistry.Producer.Snapshot.ProjectionHost.GradeSeparatedJunction
{
    using BackOffice;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates;
    using Microsoft.EntityFrameworkCore;

    public class GradeSeparatedJunctionProducerSnapshotContext : RunnerDbContext<GradeSeparatedJunctionProducerSnapshotContext>
    {
        public GradeSeparatedJunctionProducerSnapshotContext()
        {
        }

        // This needs to be DbContextOptions<T> for Autofac!
        public GradeSeparatedJunctionProducerSnapshotContext(DbContextOptions<GradeSeparatedJunctionProducerSnapshotContext> options)
            : base(options)
        {
        }

        public override string ProjectionStateSchema => WellKnownSchemas.GradeSeparatedJunctionProducerSnapshotMetaSchema;
        public DbSet<GradeSeparatedJunctionRecord> GradeSeparatedJunctions { get; set; }

        protected override void OnConfiguringOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseRoadRegistryInMemorySqlServer();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //base.OnModelCreating(modelBuilder); DO NOT trigger base => Does assembly scan!
            modelBuilder.ApplyConfiguration(new ProjectionStatesConfiguration(ProjectionStateSchema));
            modelBuilder.ApplyConfiguration(new GradeSeparatedJunctionConfiguration());
        }
    }
}
