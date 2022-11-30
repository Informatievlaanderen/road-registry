namespace RoadRegistry.Producer.Snapshot.ProjectionHost.RoadNode
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Hosts;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;

    public class RoadNodeProducerSnapshotContextMigrationFactory : RunnerDbContextMigrationFactory<RoadNodeProducerSnapshotContext>
    {
        public RoadNodeProducerSnapshotContextMigrationFactory() :
            base(WellknownConnectionNames.ProducerSnapshotProjectionsAdmin, HistoryConfiguration)
        {
        }

        private static MigrationHistoryConfiguration HistoryConfiguration =>
            new()
            {
                Schema = WellknownSchemas.RoadNodeProducerSnapshotSchema,
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
