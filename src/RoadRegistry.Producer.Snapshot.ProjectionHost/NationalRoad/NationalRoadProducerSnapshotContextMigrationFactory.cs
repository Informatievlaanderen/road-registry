namespace RoadRegistry.Producer.Snapshot.ProjectionHost.NationalRoad
{
    using BackOffice;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;

    public class NationalRoadProducerSnapshotContextMigrationFactory : RunnerDbContextMigrationFactory<NationalRoadProducerSnapshotContext>
    {
        public NationalRoadProducerSnapshotContextMigrationFactory() :
            base(WellknownConnectionNames.ProducerSnapshotProjectionsAdmin, HistoryConfiguration)
        {
        }

        private static MigrationHistoryConfiguration HistoryConfiguration =>
            new()
            {
                Schema = WellknownSchemas.NationalRoadProducerSnapshotSchema,
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
