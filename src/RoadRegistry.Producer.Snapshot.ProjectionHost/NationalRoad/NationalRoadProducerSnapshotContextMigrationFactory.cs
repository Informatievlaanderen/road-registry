namespace RoadRegistry.Producer.Snapshot.ProjectionHost.NationalRoad
{
    using BackOffice;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.SqlServer;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;

    public class NationalRoadProducerSnapshotContextMigrationFactory : SqlServerRunnerDbContextMigrationFactory<NationalRoadProducerSnapshotContext>
    {
        public NationalRoadProducerSnapshotContextMigrationFactory() :
            base(WellKnownConnectionNames.ProducerSnapshotProjectionsAdmin, HistoryConfiguration)
        {
        }

        private static MigrationHistoryConfiguration HistoryConfiguration =>
            new()
            {
                Schema = WellKnownSchemas.NationalRoadProducerSnapshotSchema,
                Table = MigrationTables.NationalRoadProducerSnapshot
            };

        protected override void ConfigureSqlServerOptions(SqlServerDbContextOptionsBuilder sqlServerOptions)
        {
            sqlServerOptions.UseNetTopologySuite();
            base.ConfigureSqlServerOptions(sqlServerOptions);
        }

        protected override NationalRoadProducerSnapshotContext CreateContext(DbContextOptions<NationalRoadProducerSnapshotContext> migrationContextOptions)
        {
            return new NationalRoadProducerSnapshotContext(migrationContextOptions);
        }
    }
}
