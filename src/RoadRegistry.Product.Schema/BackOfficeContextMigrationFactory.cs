namespace RoadRegistry.BackOffice.Schema
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Microsoft.EntityFrameworkCore;

    public class BackOfficeContextMigrationFactory : RunnerDbContextMigrationFactory<BackOfficeContext>
    {
        public BackOfficeContextMigrationFactory() :
            base(WellknownConnectionNames.BackOfficeProjectionsAdmin, HistoryConfiguration)
        { }

        private static MigrationHistoryConfiguration HistoryConfiguration =>
            new MigrationHistoryConfiguration
            {
                Schema = WellknownSchemas.BackOfficeSchema,
                Table = MigrationTables.BackOffice
            };

        protected override BackOfficeContext CreateContext(DbContextOptions<BackOfficeContext> migrationContextOptions)
            => new BackOfficeContext(migrationContextOptions);
    }
}
