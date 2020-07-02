namespace RoadRegistry.Syndication.Schema
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.EntityFrameworkCore.Infrastructure.Internal;

    public class SyndicationContextMigrationFactory : RunnerDbContextMigrationFactory<SyndicationContext>
    {
        public SyndicationContextMigrationFactory() :
            base(WellknownConnectionNames.SyndicationProjectionsAdmin, HistoryConfiguration)
        { }

        private static MigrationHistoryConfiguration HistoryConfiguration =>
            new MigrationHistoryConfiguration
            {
                Schema = WellknownSchemas.SyndicationSchema,
                Table = MigrationTables.Syndication
            };

        protected override SyndicationContext CreateContext(DbContextOptions<SyndicationContext> migrationContextOptions)
            => new SyndicationContext(migrationContextOptions);

        protected override void ConfigureSqlServerOptions(SqlServerDbContextOptionsBuilder sqlServerOptions)
        {
            sqlServerOptions.UseNetTopologySuite();
            base.ConfigureSqlServerOptions(sqlServerOptions);
        }
    }
}
