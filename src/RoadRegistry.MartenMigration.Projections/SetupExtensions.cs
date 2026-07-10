namespace RoadRegistry.MartenMigration.Projections;

using System;
using BackOffice;
using Hosts.Infrastructure.Extensions;
using Infrastructure.MartenDb.Setup;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public static class SetupExtensions
{
    public static void AddMartenDbMigrationEventProcessor(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(WellKnownConnectionNames.Marten);
        if (string.IsNullOrEmpty(connectionString))
        {
            services.AddSingleton<Action>(sp =>
            {
                return () =>
                {
                    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                    loggerFactory.CreateLogger("MartenDbMigrationEventProcessor").LogError("Marten migration event processor is disabled.");
                };
            });
            return;
        }

        services
            .AddSingleton<ConfigureDbContextOptionsBuilder<MartenMigrationContext>>(MartenMigrationContext.ConfigureOptions)
            .AddDbContext<MartenMigrationContext>((sp, options) => sp.GetRequiredService<ConfigureDbContextOptionsBuilder<MartenMigrationContext>>()(sp, options))
            .AddDbContextFactory<MartenMigrationContext>((sp, options) => sp.GetRequiredService<ConfigureDbContextOptionsBuilder<MartenMigrationContext>>()(sp, options))
            ;

        services
            .AddMartenRoad(options => options
                .AddRoadNetworkTopologyProjection()
                .AddRoadAggregatesSnapshots())
            .Services.AddDatabaseMigrations();

        services
            .AddDbContextEventProcessorServices<MartenMigrationContextEventProcessor, MartenMigrationContext>(sp => [new MartenMigrationProjection(sp.GetRequiredService<IDocumentStore>())])
            .AddHostedService<MartenMigrationContextEventProcessor>();
    }
}
