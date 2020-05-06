namespace RoadRegistry.Editor.Schema
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Microsoft.EntityFrameworkCore;

    public class EditorContextMigrationFactory : RunnerDbContextMigrationFactory<EditorContext>
    {
        public EditorContextMigrationFactory() :
            base(WellknownConnectionNames.EditorProjectionsAdmin, HistoryConfiguration)
        { }

        private static MigrationHistoryConfiguration HistoryConfiguration =>
            new MigrationHistoryConfiguration
            {
                Schema = WellknownSchemas.EditorSchema,
                Table = MigrationTables.Editor
            };

        protected override EditorContext CreateContext(DbContextOptions<EditorContext> migrationContextOptions)
            => new EditorContext(migrationContextOptions);
    }
}
