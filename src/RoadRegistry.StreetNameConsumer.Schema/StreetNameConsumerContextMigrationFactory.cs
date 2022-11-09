namespace RoadRegistry.StreetNameConsumer.Schema;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Hosts;
using Microsoft.EntityFrameworkCore;

public class StreetNameConsumerContextMigrationFactory : RunnerDbContextMigrationFactory<StreetNameConsumerContext>
{
    public StreetNameConsumerContextMigrationFactory()
        : this(WellknownConnectionNames.StreetNameConsumerAdmin)
    {
    }

    public StreetNameConsumerContextMigrationFactory(string connectionStringName)
        : base(connectionStringName, new MigrationHistoryConfiguration
        {
            Schema = WellknownSchemas.StreetNameConsumerSchema,
            Table = MigrationTables.StreetNameConsumer
        })
    {
    }

    public StreetNameConsumerContext Create(DbContextOptions<StreetNameConsumerContext> options)
    {
        return CreateContext(options);
    }

    protected override StreetNameConsumerContext CreateContext(DbContextOptions<StreetNameConsumerContext> migrationContextOptions)
    {
        return new StreetNameConsumerContext(migrationContextOptions);
    }
}