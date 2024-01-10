namespace RoadRegistry.Producer.Snapshot.ProjectionHost.NationalRoad
{
    using BackOffice;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates;
    using Microsoft.EntityFrameworkCore;

    public class NationalRoadProducerSnapshotContext : RunnerDbContext<NationalRoadProducerSnapshotContext>
    {
        public NationalRoadProducerSnapshotContext()
        {
        }

        // This needs to be DbContextOptions<T> for Autofac!
        public NationalRoadProducerSnapshotContext(DbContextOptions<NationalRoadProducerSnapshotContext> options)
            : base(options)
        {
        }

        public override string ProjectionStateSchema => WellknownSchemas.NationalRoadProducerSnapshotMetaSchema;
        public DbSet<NationalRoadRecord> NationalRoads { get; set; }

        protected override void OnConfiguringOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseRoadRegistryInMemorySqlServer();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //base.OnModelCreating(modelBuilder); DO NOT trigger base => Does assembly scan!
            modelBuilder.ApplyConfiguration(new ProjectionStatesConfiguration(ProjectionStateSchema));
            modelBuilder.ApplyConfiguration(new NationalRoadConfiguration());
        }
    }
}
