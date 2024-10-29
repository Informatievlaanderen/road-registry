namespace RoadRegistry.Sync.MunicipalityRegistry;

using System;
using BackOffice;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class MunicipalityEventConsumerContext : ConsumerDbContext<MunicipalityEventConsumerContext>
{
    private const string ConsumerSchema = WellKnownSchemas.MunicipalityEventConsumerSchema;

    public MunicipalityEventConsumerContext()
    {
    }

    // This needs to be DbContextOptions<T> for Autofac!
    public MunicipalityEventConsumerContext(DbContextOptions<MunicipalityEventConsumerContext> options)
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
                sp.GetRequiredService<IConfiguration>().GetRequiredConnectionString(WellKnownConnectionNames.MunicipalityEventConsumer),
                sqlOptions => sqlOptions
                    .EnableRetryOnFailure()
                    .MigrationsHistoryTable(MigrationTables.MunicipalityEventConsumer, WellKnownSchemas.MunicipalityEventConsumerSchema));
    }
}

public class MunicipalityEventConsumerContextMigrationFactory : DbContextMigratorFactory<MunicipalityEventConsumerContext>
{
    public MunicipalityEventConsumerContextMigrationFactory()
        : base(WellKnownConnectionNames.MunicipalityEventConsumerAdmin, new MigrationHistoryConfiguration
        {
            Schema = WellKnownSchemas.MunicipalityEventConsumerSchema,
            Table = MigrationTables.MunicipalityEventConsumer
        })
    {
    }

    protected override MunicipalityEventConsumerContext CreateContext(DbContextOptions<MunicipalityEventConsumerContext> migrationContextOptions)
    {
        return new MunicipalityEventConsumerContext(migrationContextOptions);
    }
}
