namespace RoadRegistry.CommandHandling;

using Actions.ChangeRoadNetwork;
using Actions.RemoveRoadSegments;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRoadRegistryCommandHandlers(this IServiceCollection services)
    {
        return services
            .AddScoped<ChangeRoadNetworkCommandHandler>()
            .AddScoped<RemoveRoadSegmentsCommandHandler>()
            ;
    }
}
