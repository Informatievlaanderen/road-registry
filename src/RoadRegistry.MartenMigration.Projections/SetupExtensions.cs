namespace RoadRegistry.MartenMigration.Projections;

using BackOffice;
using Hosts.Infrastructure.Extensions;
using Infrastructure.MartenDb.Setup;
using Marten;
using Microsoft.Extensions.DependencyInjection;

public static class SetupExtensions
{
    public static void AddMartenDbMigrationEventProcessor(this IServiceCollection services)
    {
        services
            .AddSingleton<ConfigureDbContextOptionsBuilder<MartenMigrationContext>>(MartenMigrationContext.ConfigureOptions)
            .AddDbContext<MartenMigrationContext>((sp, options) => sp.GetRequiredService<ConfigureDbContextOptionsBuilder<MartenMigrationContext>>()(sp, options))
            .AddDbContextFactory<MartenMigrationContext>((sp, options) => sp.GetRequiredService<ConfigureDbContextOptionsBuilder<MartenMigrationContext>>()(sp, options))
            ;

        services
            .AddSingleton<MigrationRoadNetworkRepository>()
            .AddMartenRoad(options => options.AddMartenDbMigration());

        services
            .AddDbContextEventProcessorServices<MartenMigrationContextEventProcessor, MartenMigrationContext>(sp => [new MartenMigrationProjection(sp.GetRequiredService<MigrationRoadNetworkRepository>())])
            .AddHostedService<MartenMigrationContextEventProcessor>();
    }

    private static void AddMartenDbMigration(this StoreOptions options)
    {
        options.Schema.For<MigratedEvent>()
            .Identity(x => x.EventIdentifier);
    }
}
