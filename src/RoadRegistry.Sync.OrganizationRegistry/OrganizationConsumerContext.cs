namespace RoadRegistry.Sync.OrganizationRegistry;

using BackOffice;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

public class OrganizationConsumerContext : ConsumerDbContext<OrganizationConsumerContext>, IProjectionStatesDbContext
{
    public const string ConsumerSchema = WellKnownSchemas.OrganizationConsumerSchema;

    public OrganizationConsumerContext()
    {
    }

    // This needs to be DbContextOptions<T> for Autofac!
    public OrganizationConsumerContext(DbContextOptions<OrganizationConsumerContext> options)
        : base(options)
    {
    }

    public DbSet<ProjectionStateItem> ProjectionStates => Set<ProjectionStateItem>();
    
    protected override void OnConfiguringOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseRoadRegistryInMemorySqlServer();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new ProcessedMessageConfiguration(ConsumerSchema));
        modelBuilder.ApplyConfiguration(new ProjectionStatesConfiguration(ConsumerSchema));
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }

    public static void ConfigureOptions(IServiceProvider sp, DbContextOptionsBuilder options)
    {
        options
            .UseLoggerFactory(sp.GetService<ILoggerFactory>())
            .UseSqlServer(
                sp.GetRequiredService<IConfiguration>().GetRequiredConnectionString(WellKnownConnectionNames.OrganizationConsumerProjections),
                sqlOptions => sqlOptions
                    .EnableRetryOnFailure()
                    .MigrationsHistoryTable(MigrationTables.OrganizationConsumer, ConsumerSchema));
    }
}

public class OrganizationConsumerContextMigratorFactory : DbContextMigratorFactory<OrganizationConsumerContext>
{
    public OrganizationConsumerContextMigratorFactory()
        : base(WellKnownConnectionNames.OrganizationConsumerProjectionsAdmin, new MigrationHistoryConfiguration
        {
            Schema = WellKnownSchemas.OrganizationConsumerSchema,
            Table = MigrationTables.OrganizationConsumer
        })
    {
    }

    protected override OrganizationConsumerContext CreateContext(DbContextOptions<OrganizationConsumerContext> migrationContextOptions)
    {
        return new OrganizationConsumerContext(migrationContextOptions);
    }
}
