namespace RoadRegistry.StreetNameConsumer.Schema;

using BackOffice;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Microsoft.EntityFrameworkCore;

public class StreetNameConsumerContextMigrationFactory : RunnerDbContextMigrationFactory<StreetNameConsumerContext>
{
    public StreetNameConsumerContextMigrationFactory()
        : this(WellknownConnectionNames.StreetNameConsumerProjectionsAdmin)
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
