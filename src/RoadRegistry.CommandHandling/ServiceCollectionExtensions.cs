namespace RoadRegistry.CommandHandling;

using Actions.ChangeRoadNetwork;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddChangeRoadNetworkCommandHandler(this IServiceCollection services)
    {
        return services
            .AddSingleton<RoadNetworkChangesFactory>()
            .AddScoped<ChangeRoadNetworkCommandHandler>()
            ;
    }
}
