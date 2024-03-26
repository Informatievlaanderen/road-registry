namespace RoadRegistry.Producer.Snapshot.ProjectionHost.GradeSeparatedJunction
{
    using BackOffice;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.SqlServer;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;

    public class GradeSeparatedJunctionProducerSnapshotContextMigrationFactory : SqlServerRunnerDbContextMigrationFactory<GradeSeparatedJunctionProducerSnapshotContext>
    {
        public GradeSeparatedJunctionProducerSnapshotContextMigrationFactory() :
            base(WellKnownConnectionNames.ProducerSnapshotProjectionsAdmin, HistoryConfiguration)
        {
        }

        private static MigrationHistoryConfiguration HistoryConfiguration =>
            new()
            {
                Schema = WellKnownSchemas.GradeSeparatedJunctionProducerSnapshotSchema,
                Table = MigrationTables.GradeSeparatedJunctionProducerSnapshot
            };

        protected override void ConfigureSqlServerOptions(SqlServerDbContextOptionsBuilder sqlServerOptions)
        {
            sqlServerOptions.UseNetTopologySuite();
            base.ConfigureSqlServerOptions(sqlServerOptions);
        }

        protected override GradeSeparatedJunctionProducerSnapshotContext CreateContext(DbContextOptions<GradeSeparatedJunctionProducerSnapshotContext> migrationContextOptions)
        {
            return new GradeSeparatedJunctionProducerSnapshotContext(migrationContextOptions);
        }
    }
}
