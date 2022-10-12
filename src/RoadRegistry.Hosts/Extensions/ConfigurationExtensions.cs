using System;

namespace RoadRegistry.Extensions.Configuration
{
    using Microsoft.Extensions.Configuration;

    public static class ConfigurationExtensions
    {
        public static T GetRequiredValue<T>(this IConfiguration configuration, string key)
        {
            return configuration.GetValue<T>(key) ?? throw new InvalidOperationException($"The {key} configuration variable was not set.");
        }
    }
}
