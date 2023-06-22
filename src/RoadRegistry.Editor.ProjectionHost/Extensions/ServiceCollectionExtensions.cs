namespace RoadRegistry.Editor.ProjectionHost.Extensions;

using System;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Hosts;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDbContextEventProcessor<TDbContext, TDbContextEventProcessor>(this IServiceCollection services,
        Func<IServiceProvider, ConnectedProjection<TDbContext>[]> projections)
        where TDbContext : RunnerDbContext<TDbContext>
        where TDbContextEventProcessor : DbContextEventProcessor<TDbContext>
    {
        services
            .AddSingleton(sp => new DbContextEventProcessorProjections<TDbContextEventProcessor, TDbContext>(projections(sp)))
            .AddHostedService<TDbContextEventProcessor>();

        return services;
    }
}
