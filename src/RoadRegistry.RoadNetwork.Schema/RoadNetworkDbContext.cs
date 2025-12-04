namespace RoadRegistry.RoadNetwork.Schema
{
    using BackOffice;
    using Microsoft.EntityFrameworkCore;

    public class RoadNetworkDbContext : DbContext
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
                .UseRoadRegistryInMemorySqlServer();
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

        public async Task<int> GetNextSequenceValueAsync(string name)
        {
            await Database.OpenConnectionAsync();
            var cmd = Database.GetDbConnection().CreateCommand();
            cmd.CommandText = $"SELECT NEXT VALUE FOR {Schema}.{name};";
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public int GetNextSequenceValue(string name)
        {
            Database.OpenConnection();
            var cmd = Database.GetDbConnection().CreateCommand();
            cmd.CommandText = $"SELECT NEXT VALUE FOR {Schema}.{name};";
            var result = cmd.ExecuteScalar();
            return Convert.ToInt32(result);
        }
    }

    public class RoadNetworkDbContextMigrationFactory : DbContextMigratorFactory<RoadNetworkDbContext>
    {
        public RoadNetworkDbContextMigrationFactory()
            : base(WellKnownConnectionNames.CommandHostAdmin, new MigrationHistoryConfiguration
            {
                Schema = RoadNetworkDbContext.Schema,
                Table = MigrationTables.Default
            })
        {
        }

        protected override RoadNetworkDbContext CreateContext(DbContextOptions<RoadNetworkDbContext> migrationContextOptions)
        {
            return new RoadNetworkDbContext(migrationContextOptions);
        }
    }
}
