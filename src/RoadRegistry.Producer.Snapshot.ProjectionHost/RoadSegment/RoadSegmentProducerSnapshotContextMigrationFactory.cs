namespace RoadRegistry.Producer.Snapshot.ProjectionHost.RoadSegment
{
    using BackOffice;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.SqlServer;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;

    public class RoadSegmentProducerSnapshotContextMigrationFactory : SqlServerRunnerDbContextMigrationFactory<RoadSegmentProducerSnapshotContext>
    {
        public RoadSegmentProducerSnapshotContextMigrationFactory() :
            base(WellKnownConnectionNames.ProducerSnapshotProjectionsAdmin, HistoryConfiguration)
        {
        }

        private static MigrationHistoryConfiguration HistoryConfiguration =>
            new()
            {
                Schema = WellKnownSchemas.RoadSegmentProducerSnapshotSchema,
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
