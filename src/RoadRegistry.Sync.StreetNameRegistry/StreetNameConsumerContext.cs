namespace RoadRegistry.StreetNameConsumer.Schema;

using BackOffice;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
using Microsoft.EntityFrameworkCore;

public class StreetNameConsumerContext : ConsumerDbContext<StreetNameConsumerContext>
{
    private const string ConsumerSchema = WellknownSchemas.StreetNameConsumerSchema;

    public StreetNameConsumerContext()
    {
    }

    // This needs to be DbContextOptions<T> for Autofac!
    public StreetNameConsumerContext(DbContextOptions<StreetNameConsumerContext> options)
        : base(options)
    {
    }
    
    public DbSet<StreetNameConsumerItem> StreetNames { get; set; }

    protected override void OnConfiguringOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseRoadRegistryInMemorySqlServer();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        modelBuilder.ApplyConfiguration(new ProcessedMessageConfiguration(ConsumerSchema));
    }
}

public class StreetNameConsumerContextMigrationFactory : DbContextMigratorFactory<StreetNameConsumerContext>
{
    public StreetNameConsumerContextMigrationFactory()
        : base(WellknownConnectionNames.StreetNameConsumerProjectionsAdmin, new MigrationHistoryConfiguration
        {
            Schema = WellknownSchemas.StreetNameConsumerSchema,
            Table = MigrationTables.StreetNameConsumer
        })
    {
    }
    
    protected override StreetNameConsumerContext CreateContext(DbContextOptions<StreetNameConsumerContext> migrationContextOptions)
    {
        return new StreetNameConsumerContext(migrationContextOptions);
    }
}
