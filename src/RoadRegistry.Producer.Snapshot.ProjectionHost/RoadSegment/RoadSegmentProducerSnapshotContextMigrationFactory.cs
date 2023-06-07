namespace RoadRegistry.Producer.Snapshot.ProjectionHost.RoadSegment
{
    using BackOffice;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;

    public class RoadSegmentProducerSnapshotContextMigrationFactory : RunnerDbContextMigrationFactory<RoadSegmentProducerSnapshotContext>
    {
        public RoadSegmentProducerSnapshotContextMigrationFactory() :
            base(WellknownConnectionNames.ProducerSnapshotProjectionsAdmin, HistoryConfiguration)
        {
        }

        private static MigrationHistoryConfiguration HistoryConfiguration =>
            new()
            {
                Schema = WellknownSchemas.RoadSegmentProducerSnapshotSchema,
                Table = MigrationTables.RoadSegmentProducerSnapshot
            };

        protected override void ConfigureSqlServerOptions(SqlServerDbContextOptionsBuilder sqlServerOptions)
        {
            sqlServerOptions.UseNetTopologySuite();
            base.ConfigureSqlServerOptions(sqlServerOptions);
        }

        protected override RoadSegmentProducerSnapshotContext CreateContext(DbContextOptions<RoadSegmentProducerSnapshotContext> migrationContextOptions)
        {
            return new RoadSegmentProducerSnapshotContext(migrationContextOptions);
        }
    }
}
