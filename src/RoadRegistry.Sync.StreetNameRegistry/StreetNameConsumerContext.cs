namespace RoadRegistry.Sync.StreetNameRegistry;

using System;
using BackOffice;
using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Sql.EntityFrameworkCore;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class StreetNameConsumerContext : ConsumerDbContext<StreetNameConsumerContext>
{
    private const string ConsumerSchema = WellKnownSchemas.StreetNameConsumerSchema;

    public StreetNameConsumerContext()
    {
    }

    // This needs to be DbContextOptions<T> for Autofac!
    public StreetNameConsumerContext(DbContextOptions<StreetNameConsumerContext> options)
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
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .UseSqlServer(
                sp.GetRequiredService<TraceDbConnection<StreetNameConsumerContext>>(),
                sqlOptions => sqlOptions
                    .EnableRetryOnFailure()
                    .MigrationsHistoryTable(MigrationTables.StreetNameConsumer, WellKnownSchemas.StreetNameConsumerSchema));
    }
}

public class StreetNameConsumerContextMigrationFactory : DbContextMigratorFactory<StreetNameConsumerContext>
{
    public StreetNameConsumerContextMigrationFactory()
        : base(WellKnownConnectionNames.StreetNameConsumerAdmin, new MigrationHistoryConfiguration
        {
            Schema = WellKnownSchemas.StreetNameConsumerSchema,
            Table = MigrationTables.StreetNameConsumer
        })
    {
    }
    
    protected override StreetNameConsumerContext CreateContext(DbContextOptions<StreetNameConsumerContext> migrationContextOptions)
    {
        return new StreetNameConsumerContext(migrationContextOptions);
    }
}
