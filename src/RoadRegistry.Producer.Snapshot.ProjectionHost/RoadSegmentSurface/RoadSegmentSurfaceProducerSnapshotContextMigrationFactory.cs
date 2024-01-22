namespace RoadRegistry.Producer.Snapshot.ProjectionHost.RoadSegmentSurface
{
    using BackOffice;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;

    public class RoadSegmentSurfaceProducerSnapshotContextMigrationFactory : RunnerDbContextMigrationFactory<RoadSegmentSurfaceProducerSnapshotContext>
    {
        public RoadSegmentSurfaceProducerSnapshotContextMigrationFactory() :
            base(WellKnownConnectionNames.ProducerSnapshotProjectionsAdmin, HistoryConfiguration)
        {
        }

        private static MigrationHistoryConfiguration HistoryConfiguration =>
            new()
            {
                Schema = WellKnownSchemas.RoadSegmentSurfaceProducerSnapshotSchema,
                Table = MigrationTables.RoadSegmentSurfaceProducerSnapshot
            };

        protected override void ConfigureSqlServerOptions(SqlServerDbContextOptionsBuilder sqlServerOptions)
        {
            sqlServerOptions.UseNetTopologySuite();
            base.ConfigureSqlServerOptions(sqlServerOptions);
        }

        protected override RoadSegmentSurfaceProducerSnapshotContext CreateContext(DbContextOptions<RoadSegmentSurfaceProducerSnapshotContext> migrationContextOptions)
        {
            return new RoadSegmentSurfaceProducerSnapshotContext(migrationContextOptions);
        }
    }
}
