namespace RoadRegistry.Editor.ProjectionHost.Extensions;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Hosts.Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Schema;
using System;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEditorContextEventProcessor<TDbContextEventProcessor>(this IServiceCollection services,
        Func<IServiceProvider, ConnectedProjection<EditorContext>[]> projections)
        where TDbContextEventProcessor : EditorContextEventProcessor
    {
        services
            .AddDbContextEventProcessorServices<TDbContextEventProcessor, EditorContext>(projections)
            .AddHostedService<TDbContextEventProcessor>();

        return services;
    }
}
