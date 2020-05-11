namespace RoadRegistry.Wms.Schema
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Microsoft.EntityFrameworkCore;

    public class WmsContextMigrationFactory : RunnerDbContextMigrationFactory<WmsContext>
    {
        public WmsContextMigrationFactory() :
            base(WellknownConnectionNames.WmsProjectionsAdmin, HistoryConfiguration)
        { }

        private static MigrationHistoryConfiguration HistoryConfiguration =>
            new MigrationHistoryConfiguration
            {
                Schema = WellknownSchemas.WmsSchema,
                Table = MigrationTables.Wms
            };

        protected override WmsContext CreateContext(DbContextOptions<WmsContext> migrationContextOptions)
            => new WmsContext(migrationContextOptions);
    }
}
