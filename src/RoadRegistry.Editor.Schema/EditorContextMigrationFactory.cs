namespace RoadRegistry.Editor.Schema;

using BackOffice;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

public class EditorContextMigrationFactory : RunnerDbContextMigrationFactory<EditorContext>
{
    public EditorContextMigrationFactory()
        : base(WellKnownConnectionNames.EditorProjectionsAdmin, HistoryConfiguration)
    {
    }

    private static MigrationHistoryConfiguration HistoryConfiguration =>
        new()
        {
            Schema = WellKnownSchemas.EditorSchema,
            Table = MigrationTables.Editor
        };

    protected override void ConfigureSqlServerOptions(SqlServerDbContextOptionsBuilder sqlServerOptions)
    {
        sqlServerOptions.UseNetTopologySuite();
        base.ConfigureSqlServerOptions(sqlServerOptions);
    }

    protected override EditorContext CreateContext(DbContextOptions<EditorContext> migrationContextOptions)
    {
        return new EditorContext(migrationContextOptions);
    }
}
