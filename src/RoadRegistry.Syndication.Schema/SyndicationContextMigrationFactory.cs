namespace RoadRegistry.Syndication.Schema;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Hosts;
using Microsoft.EntityFrameworkCore;

public class SyndicationContextMigrationFactory : RunnerDbContextMigrationFactory<SyndicationContext>
{
    public SyndicationContextMigrationFactory() :
        base(WellknownConnectionNames.SyndicationProjectionsAdmin, HistoryConfiguration)
    {
    }

    protected override SyndicationContext CreateContext(DbContextOptions<SyndicationContext> migrationContextOptions)
    {
        return new SyndicationContext(migrationContextOptions);
    }

    private static MigrationHistoryConfiguration HistoryConfiguration =>
        new()
        {
            Schema = WellknownSchemas.SyndicationSchema,
            Table = MigrationTables.Syndication
        };
}