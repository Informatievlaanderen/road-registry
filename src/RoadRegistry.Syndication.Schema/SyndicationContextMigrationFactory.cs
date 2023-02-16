namespace RoadRegistry.Syndication.Schema;

using BackOffice;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Microsoft.EntityFrameworkCore;

public class SyndicationContextMigrationFactory : RunnerDbContextMigrationFactory<SyndicationContext>
{
    public SyndicationContextMigrationFactory() :
        base(WellknownConnectionNames.SyndicationProjectionsAdmin, HistoryConfiguration)
    {
    }

    private static MigrationHistoryConfiguration HistoryConfiguration =>
        new()
        {
            Schema = WellknownSchemas.SyndicationSchema,
            Table = MigrationTables.Syndication
        };

    protected override SyndicationContext CreateContext(DbContextOptions<SyndicationContext> migrationContextOptions)
    {
        return new SyndicationContext(migrationContextOptions);
    }
}
