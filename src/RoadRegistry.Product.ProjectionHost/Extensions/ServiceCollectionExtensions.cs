namespace RoadRegistry.Product.ProjectionHost.Extensions;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Hosts.Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;
using RoadRegistry.Product.ProjectionHost.EventProcessors;
using Schema;
using System;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProductContextEventProcessor<TDbContextEventProcessor>(this IServiceCollection services,
        Func<IServiceProvider, ConnectedProjection<ProductContext>[]> projections)
        where TDbContextEventProcessor : ProductContextEventProcessor
    {
        services
            .AddDbContextEventProcessorServices<TDbContextEventProcessor, ProductContext>(projections)
            .AddHostedService<TDbContextEventProcessor>();

        return services;
    }
}
