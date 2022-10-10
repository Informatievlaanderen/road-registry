namespace RoadRegistry.BackOffice.MessagingHost.Kafka;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Microsoft.EntityFrameworkCore;
using RoadRegistry.BackOffice.MessagingHost.Kafka.StreetName;
using RoadRegistry.Hosts;

public class StreetNameConsumerContext : RunnerDbContext<StreetNameConsumerContext>
{
    public StreetNameConsumerContext()
    {
    }

    // This needs to be DbContextOptions<T> for Autofac!
    public StreetNameConsumerContext(DbContextOptions<StreetNameConsumerContext> options)
        : base(options)
    {
    }

    public override string ProjectionStateSchema => WellknownSchemas.StreetNameConsumerSchema;

    public DbSet<StreetNameConsumerItem> StreetNames { get; set; }

    protected override void OnConfiguringOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=EFProviders.InMemory.RoadRegistry.RoadRegistryContext;Trusted_Connection=True;");
    }
}

public class StreetNameConsumerContextFactory : RunnerDbContextMigrationFactory<StreetNameConsumerContext>
{
    public StreetNameConsumerContextFactory()
        : this(WellknownConnectionNames.StreetNameConsumerAdmin)
    { }

    public StreetNameConsumerContextFactory(string connectionStringName)
        : base(connectionStringName, new MigrationHistoryConfiguration
        {
            Schema = WellknownSchemas.StreetNameConsumerSchema,
            Table = MigrationTables.StreetNameConsumer
        })
    { }

    protected override StreetNameConsumerContext CreateContext(DbContextOptions<StreetNameConsumerContext> migrationContextOptions) => new(migrationContextOptions);

    public StreetNameConsumerContext Create(DbContextOptions<StreetNameConsumerContext> options) => CreateContext(options);
}
