namespace RoadRegistry.Syndication.Schema;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Hosts;
using Microsoft.EntityFrameworkCore;
using StreetNameConsumer.Schema;

public class StreetNameConsumerContextMigrationFactory : RunnerDbContextMigrationFactory<StreetNameConsumerContext>
{
    public StreetNameConsumerContextMigrationFactory() :
        base(WellknownConnectionNames.StreetNameConsumerAdmin, HistoryConfiguration)
    {
    }

    private static MigrationHistoryConfiguration HistoryConfiguration =>
        new()
        {
            Schema = WellknownSchemas.StreetNameConsumerSchema,
            Table = MigrationTables.StreetNameConsumer
        };

    protected override StreetNameConsumerContext CreateContext(DbContextOptions<StreetNameConsumerContext> migrationContextOptions)
    {
        return new StreetNameConsumerContext(migrationContextOptions);
    }
}
