namespace RoadRegistry.StreetName
{
    using BackOffice.Extensions;
    using BackOffice.FeatureToggles;
    using Microsoft.Extensions.DependencyInjection;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddStreetNameClient(this IServiceCollection services)
        {
            return services
                .RegisterOptions<StreetNameRegistryOptions>()
                .AddSingleton<StreetNameApiClient>()
                .AddSingleton<StreetNameCacheClient>()
                .AddSingleton<IStreetNameClient>(sp => sp.GetRequiredService<UseKafkaStreetNameCacheFeatureToggle>().FeatureEnabled
                    ? sp.GetRequiredService<StreetNameApiClient>()
                    : sp.GetRequiredService<StreetNameCacheClient>())
                ;
        }
    }
}
