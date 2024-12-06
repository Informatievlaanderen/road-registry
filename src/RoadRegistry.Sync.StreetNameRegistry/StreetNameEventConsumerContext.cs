namespace RoadRegistry.Sync.StreetNameRegistry;

using BackOffice;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer.SqlServer;

public class StreetNameEventConsumerContext : SqlServerConsumerDbContext<StreetNameEventConsumerContext>, IOffsetOverrideDbSet
{
    private const string ConsumerSchema = WellKnownSchemas.StreetNameEventConsumerSchema;

    public override string ProcessedMessagesSchema => ConsumerSchema;

    public DbSet<OffsetOverride> OffsetOverrides => Set<OffsetOverride>();

    public StreetNameEventConsumerContext()
    {
    }

    // This needs to be DbContextOptions<T> for Autofac!
    public StreetNameEventConsumerContext(DbContextOptions<StreetNameEventConsumerContext> options)
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

        modelBuilder.ApplyConfiguration(new OffsetOverrideConfiguration(ConsumerSchema));
    }

    public static void ConfigureOptions(IServiceProvider sp, DbContextOptionsBuilder options)
    {
        options
            .UseLoggerFactory(sp.GetService<ILoggerFactory>())
            .UseSqlServer(
                sp.GetRequiredService<IConfiguration>().GetRequiredConnectionString(WellKnownConnectionNames.StreetNameEventConsumer),
                sqlOptions => sqlOptions
                    .EnableRetryOnFailure()
                    .MigrationsHistoryTable(MigrationTables.StreetNameEventConsumer, WellKnownSchemas.StreetNameEventConsumerSchema));
    }
}

public class StreetNameEventConsumerContextMigrationFactory : DbContextMigratorFactory<StreetNameEventConsumerContext>
{
    public StreetNameEventConsumerContextMigrationFactory()
        : base(WellKnownConnectionNames.StreetNameEventConsumerAdmin, new MigrationHistoryConfiguration
        {
            Schema = WellKnownSchemas.StreetNameEventConsumerSchema,
            Table = MigrationTables.StreetNameEventConsumer
        })
    {
    }

    protected override StreetNameEventConsumerContext CreateContext(DbContextOptions<StreetNameEventConsumerContext> migrationContextOptions)
    {
        return new StreetNameEventConsumerContext(migrationContextOptions);
    }
}
