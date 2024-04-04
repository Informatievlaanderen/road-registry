namespace RoadRegistry.Sync.StreetNameRegistry;

using BackOffice;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

public class StreetNameSnapshotConsumerContext : ConsumerDbContext<StreetNameSnapshotConsumerContext>
{
    private const string ConsumerSchema = WellKnownSchemas.StreetNameSnapshotConsumerSchema;

    public StreetNameSnapshotConsumerContext()
    {
    }

    // This needs to be DbContextOptions<T> for Autofac!
    public StreetNameSnapshotConsumerContext(DbContextOptions<StreetNameSnapshotConsumerContext> options)
        : base(options)
    {
    }
    
    protected override void OnConfiguringOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseRoadRegistryInMemorySqlServer();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new ProcessedMessageConfiguration(ConsumerSchema));
    }

    public static void ConfigureOptions(IServiceProvider sp, DbContextOptionsBuilder options)
    {
        options
            .UseLoggerFactory(sp.GetService<ILoggerFactory>())
            .UseSqlServer(
                sp.GetRequiredService<IConfiguration>().GetRequiredConnectionString(WellKnownConnectionNames.StreetNameSnapshotConsumer),
                sqlOptions => sqlOptions
                    .EnableRetryOnFailure()
                    .MigrationsHistoryTable(MigrationTables.StreetNameSnapshotConsumer, WellKnownSchemas.StreetNameSnapshotConsumerSchema));
    }
}

public class StreetNameSnapshotConsumerContextMigrationFactory : DbContextMigratorFactory<StreetNameSnapshotConsumerContext>
{
    public StreetNameSnapshotConsumerContextMigrationFactory()
        : base(WellKnownConnectionNames.StreetNameSnapshotConsumerAdmin, new MigrationHistoryConfiguration
        {
            Schema = WellKnownSchemas.StreetNameSnapshotConsumerSchema,
            Table = MigrationTables.StreetNameSnapshotConsumer
        })
    {
    }
    
    protected override StreetNameSnapshotConsumerContext CreateContext(DbContextOptions<StreetNameSnapshotConsumerContext> migrationContextOptions)
    {
        return new StreetNameSnapshotConsumerContext(migrationContextOptions);
    }
}
