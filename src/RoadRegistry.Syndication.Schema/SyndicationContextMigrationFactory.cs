namespace RoadRegistry.Syndication.Schema;

using BackOffice;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Microsoft.EntityFrameworkCore;

public class SyndicationContextMigrationFactory : RunnerDbContextMigrationFactory<SyndicationContext>
{
    public SyndicationContextMigrationFactory() :
        base(WellKnownConnectionNames.SyndicationProjectionsAdmin, HistoryConfiguration)
    {
    }

    private static MigrationHistoryConfiguration HistoryConfiguration =>
        new()
        {
            Schema = WellKnownSchemas.SyndicationSchema,
            Table = MigrationTables.Syndication
        };

    protected override SyndicationContext CreateContext(DbContextOptions<SyndicationContext> migrationContextOptions)
    {
        return new SyndicationContext(migrationContextOptions);
    }
}
