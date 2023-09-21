namespace RoadRegistry.Sync.OrganizationRegistry;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Microsoft.EntityFrameworkCore;
using RoadRegistry.BackOffice;

public class OrganizationConsumerContextMigrationFactory : RunnerDbContextMigrationFactory<OrganizationConsumerContext>
{
    public OrganizationConsumerContextMigrationFactory()
        : this(WellknownConnectionNames.OrganizationConsumerProjectionsAdmin)
    {
    }

    public OrganizationConsumerContextMigrationFactory(string connectionStringName)
        : base(connectionStringName, new MigrationHistoryConfiguration
        {
            Schema = WellknownSchemas.OrganizationConsumerSchema,
            Table = MigrationTables.OrganizationConsumer
        })
    {
    }

    public OrganizationConsumerContext Create(DbContextOptions<OrganizationConsumerContext> options)
    {
        return CreateContext(options);
    }

    protected override OrganizationConsumerContext CreateContext(DbContextOptions<OrganizationConsumerContext> migrationContextOptions)
    {
        return new OrganizationConsumerContext(migrationContextOptions);
    }
}
