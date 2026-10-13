namespace RoadRegistry.Producer.Snapshot.ProjectionHost.RoadNode
{
    using System;
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
            // EF's thirty second default is short for a projection batch on a loaded server, and such a timeout counts
            // as transient, so it is retried, exhausted, and ends with the message pump gone and the host coming
            // back up on the same event. Same ten minutes the editor, pbs and wms-wfs contexts already take.
            if (Database.IsRelational())
            {
                Database.SetCommandTimeout(TimeSpan.FromMinutes(10));
            }
        }

        public override string ProjectionStateSchema => WellKnownSchemas.RoadNodeProducerSnapshotMetaSchema;
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
