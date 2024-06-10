namespace RoadRegistry.Integration.ProjectionHost.Extensions;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Hosts.Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Schema;
using System;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIntegrationContextEventProcessor<TDbContextEventProcessor>(this IServiceCollection services,
        Func<IServiceProvider, ConnectedProjection<IntegrationContext>[]> projections)
        where TDbContextEventProcessor : IntegrationContextEventProcessor
    {
        services
            .AddDbContextEventProcessorServices<TDbContextEventProcessor, IntegrationContext>(projections)
            .AddHostedService<TDbContextEventProcessor>();

        return services;
    }
}
