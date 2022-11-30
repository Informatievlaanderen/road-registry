namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Schema
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Hosts;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;

    public class ProducerSnapshotContextMigrationFactory : RunnerDbContextMigrationFactory<ProducerSnapshotContext>
    {
        public ProducerSnapshotContextMigrationFactory() :
            base(WellknownConnectionNames.ProducerSnapshotProjectionsAdmin, HistoryConfiguration)
        {
        }

        private static MigrationHistoryConfiguration HistoryConfiguration =>
            new()
            {
                Schema = WellknownSchemas.ProducerSnapshotSchema,
                Table = MigrationTables.ProducerSnapshot
            };

        protected override void ConfigureSqlServerOptions(SqlServerDbContextOptionsBuilder sqlServerOptions)
        {
            sqlServerOptions.UseNetTopologySuite();
            base.ConfigureSqlServerOptions(sqlServerOptions);
        }

        protected override ProducerSnapshotContext CreateContext(DbContextOptions<ProducerSnapshotContext> migrationContextOptions)
        {
            return new ProducerSnapshotContext(migrationContextOptions);
        }
    }
}
