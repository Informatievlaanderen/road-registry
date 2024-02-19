namespace RoadRegistry.Sync.StreetNameRegistry;

using System;
using BackOffice;
using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Sql.EntityFrameworkCore;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TypeConfigurations;

public class StreetNameEventConsumerContext : ConsumerDbContext<StreetNameEventConsumerContext>
{
    private const string ConsumerSchema = WellKnownSchemas.StreetNameEventConsumerSchema;

    public StreetNameEventConsumerContext()
    {
    }

    // This needs to be DbContextOptions<T> for Autofac!
    public StreetNameEventConsumerContext(DbContextOptions<StreetNameEventConsumerContext> options)
        : base(options)
    {
    }

    public DbSet<RenamedStreetNameRecord> RenamedStreetNames { get; set; }

    protected override void OnConfiguringOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseRoadRegistryInMemorySqlServer();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new ProcessedMessageConfiguration(ConsumerSchema));
        modelBuilder.ApplyConfiguration(new RenamedStreetNameRecordEntityTypeConfiguration());
    }

    public static void ConfigureOptions(IServiceProvider sp, DbContextOptionsBuilder options)
    {
        options
            .UseLoggerFactory(sp.GetService<ILoggerFactory>())
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .UseSqlServer(
                sp.GetRequiredService<TraceDbConnection<StreetNameEventConsumerContext>>(),
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