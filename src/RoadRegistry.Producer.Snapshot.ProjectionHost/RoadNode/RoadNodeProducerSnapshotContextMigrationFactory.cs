namespace RoadRegistry.Producer.Snapshot.ProjectionHost.RoadNode
{
    using BackOffice;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;

    public class RoadNodeProducerSnapshotContextMigrationFactory : RunnerDbContextMigrationFactory<RoadNodeProducerSnapshotContext>
    {
        public RoadNodeProducerSnapshotContextMigrationFactory() :
            base(WellKnownConnectionNames.ProducerSnapshotProjectionsAdmin, HistoryConfiguration)
        {
        }

        private static MigrationHistoryConfiguration HistoryConfiguration =>
            new()
            {
                Schema = WellKnownSchemas.RoadNodeProducerSnapshotSchema,
                Table = MigrationTables.RoadNodeProducerSnapshot
            };

        protected override void ConfigureSqlServerOptions(SqlServerDbContextOptionsBuilder sqlServerOptions)
        {
            sqlServerOptions.UseNetTopologySuite();
            base.ConfigureSqlServerOptions(sqlServerOptions);
        }

        protected override RoadNodeProducerSnapshotContext CreateContext(DbContextOptions<RoadNodeProducerSnapshotContext> migrationContextOptions)
        {
            return new RoadNodeProducerSnapshotContext(migrationContextOptions);
        }
    }
}
