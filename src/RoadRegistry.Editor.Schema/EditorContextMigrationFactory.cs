namespace RoadRegistry.Editor.Schema;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Hosts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

public class EditorContextMigrationFactory : RunnerDbContextMigrationFactory<EditorContext>
{
    public EditorContextMigrationFactory() :
        base(WellknownConnectionNames.EditorProjectionsAdmin, HistoryConfiguration)
    {
    }

    private static MigrationHistoryConfiguration HistoryConfiguration =>
        new()
        {
            Schema = WellknownSchemas.EditorSchema,
            Table = MigrationTables.Editor
        };

    protected override EditorContext CreateContext(DbContextOptions<EditorContext> migrationContextOptions)
    {
        return new(migrationContextOptions);
    }

    protected override void ConfigureSqlServerOptions(SqlServerDbContextOptionsBuilder sqlServerOptions)
    {
        sqlServerOptions.UseNetTopologySuite();
        base.ConfigureSqlServerOptions(sqlServerOptions);
    }
}
