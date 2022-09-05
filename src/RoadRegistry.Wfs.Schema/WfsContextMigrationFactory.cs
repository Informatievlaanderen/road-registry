namespace RoadRegistry.Wfs.Schema
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Hosts;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;

    public class WfsContextMigrationFactory : RunnerDbContextMigrationFactory<WfsContext>
    {
        public WfsContextMigrationFactory() :
            base(WellknownConnectionNames.WfsProjectionsAdmin, HistoryConfiguration)
        { }

        private static MigrationHistoryConfiguration HistoryConfiguration =>
            new MigrationHistoryConfiguration
            {
                Schema = WellknownSchemas.WfsSchema,
                Table = MigrationTables.Wfs
            };

        protected override WfsContext CreateContext(DbContextOptions<WfsContext> migrationContextOptions)
            => new WfsContext(migrationContextOptions);

        protected override void ConfigureSqlServerOptions(SqlServerDbContextOptionsBuilder sqlServerOptions)
        {
            sqlServerOptions.UseNetTopologySuite();
            base.ConfigureSqlServerOptions(sqlServerOptions);
        }
    }
}
