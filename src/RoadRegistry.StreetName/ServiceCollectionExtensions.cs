namespace RoadRegistry.StreetName
{
    using BackOffice.Extensions;
    using Microsoft.Extensions.DependencyInjection;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddStreetNameClient(this IServiceCollection services)
        {
            return services
                .RegisterOptions<StreetNameRegistryOptions>()
                .AddSingleton<StreetNameApiClient>()
                .AddSingleton<IStreetNameClient, StreetNameApiClient>()
                ;
        }
    }
}
