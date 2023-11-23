namespace RoadRegistry.RoadNetwork.Schema
{
    using BackOffice;
    using Microsoft.EntityFrameworkCore;

    public class RoadNetworkDbContext: DbContext
    {
        public const string Schema = "RoadNetwork";

        public RoadNetworkDbContext()
        {
        }

        // This needs to be DbContextOptions<T> for Autofac!
        public RoadNetworkDbContext(DbContextOptions<RoadNetworkDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            if (optionsBuilder.IsConfigured)
            {
                return;
            }

            optionsBuilder
                .UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=EFProviders.InMemory.RoadRegistry.RoadRegistryContext;Trusted_Connection=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasSequence<int>(WellKnownDbSequences.RoadSegmentId, Schema);
            modelBuilder.HasSequence<int>(WellKnownDbSequences.EuropeanRoadAttributeId, Schema);
            modelBuilder.HasSequence<int>(WellKnownDbSequences.GradeSeparatedJunctionId, Schema);
            modelBuilder.HasSequence<int>(WellKnownDbSequences.NationalRoadAttributeId, Schema);
            modelBuilder.HasSequence<int>(WellKnownDbSequences.NumberedRoadAttributeId, Schema);
            modelBuilder.HasSequence<int>(WellKnownDbSequences.RoadNodeId, Schema);
            modelBuilder.HasSequence<int>(WellKnownDbSequences.RoadSegmentLaneAttributeId, Schema);
            modelBuilder.HasSequence<int>(WellKnownDbSequences.RoadSegmentSurfaceAttributeId, Schema);
            modelBuilder.HasSequence<int>(WellKnownDbSequences.RoadSegmentWidthAttributeId, Schema);
            modelBuilder.HasSequence<int>(WellKnownDbSequences.TransactionId, Schema);
        }
    }
}
