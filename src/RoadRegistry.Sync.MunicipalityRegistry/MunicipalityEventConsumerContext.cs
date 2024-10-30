namespace RoadRegistry.Sync.MunicipalityRegistry;

using System;
using BackOffice;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Models;

public class MunicipalityEventConsumerContext : ConsumerDbContext<MunicipalityEventConsumerContext>
{
    private const string ConsumerSchema = WellKnownSchemas.MunicipalityEventConsumerSchema;

    public DbSet<Municipality> Municipalities => Set<Municipality>();

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
        modelBuilder.ApplyConfiguration(new MunicipalityConfiguration());
    }

    public static void ConfigureOptions(IServiceProvider sp, DbContextOptionsBuilder options)
    {
        options
            .UseLoggerFactory(sp.GetService<ILoggerFactory>())
            .UseSqlServer(
                sp.GetRequiredService<IConfiguration>().GetRequiredConnectionString(WellKnownConnectionNames.MunicipalityEventConsumer),
                sqlOptions => sqlOptions
                    .EnableRetryOnFailure()
                    .UseNetTopologySuite()
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

    protected override void ConfigureSqlServerOptions(SqlServerDbContextOptionsBuilder sqlServerOptions)
    {
        base.ConfigureSqlServerOptions(sqlServerOptions);

        sqlServerOptions.UseNetTopologySuite();
    }
}
