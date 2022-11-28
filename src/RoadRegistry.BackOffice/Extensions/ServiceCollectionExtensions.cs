namespace RoadRegistry.BackOffice.Extensions;

using System;
using System.Linq;
using FeatureToggle;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFeatureToggles<TFeatureToggle>(this IServiceCollection services, IConfiguration configuration)
        where TFeatureToggle : IFeatureToggle
    {
        var featureTogglesConfiguration = configuration.GetSection("FeatureToggles");

        var applicationFeatureToggleType = typeof(TFeatureToggle);
        var featureToggles = applicationFeatureToggleType.Assembly
            .GetTypes()
            .Where(type => !type.IsAbstract && applicationFeatureToggleType.IsAssignableFrom(type))
            .Select(type =>
            {
                var configurationKey = type.Name;
                if (configurationKey.EndsWith("FeatureToggle"))
                {
                    configurationKey = configurationKey.Substring(0, configurationKey.Length - "FeatureToggle".Length);
                }

                var featureEnabled = featureTogglesConfiguration.GetValue<bool>(configurationKey);
                var featureToggle = Activator.CreateInstance(type, new object[] { featureEnabled });
                return featureToggle;
            })
            .ToList();

        foreach (var featureToggle in featureToggles)
        {
            services.AddSingleton(featureToggle.GetType(), featureToggle);
        }

        return services;
    }
}
