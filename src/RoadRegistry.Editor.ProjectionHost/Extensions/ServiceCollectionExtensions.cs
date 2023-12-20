namespace RoadRegistry.Editor.ProjectionHost.Extensions;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
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
            .AddSingleton(sp => new EditorContextEventProcessorProjections<TDbContextEventProcessor>(projections(sp)))
            .AddHostedService<TDbContextEventProcessor>();

        return services;
    }
}
