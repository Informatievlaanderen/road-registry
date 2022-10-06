using RoadRegistry.Hosts;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadRegistry.BackOffice.MessagingHost.Kafka.Infrastructure
{
    using Microsoft.Extensions.Configuration;

    public static class ConfigurationExtensions
    {
        public static IConfiguration ConfigureOptions<TOptions>(this IConfiguration configuration, string configurationSectionName) where TOptions : new()
        {
            TOptions options = new();
            var configurationSection = configuration.GetSection(configurationSectionName);

            if (configurationSection is not null)
            {
                configurationSection.Bind(options);
            }
            else
            {
                configuration.Bind(options);
            }

            return configuration;
        }

        public static IConfiguration ConfigureOptions<TOptions>(this IConfiguration configuration) where TOptions : new() => ConfigureOptions<TOptions>(configuration, typeof(TOptions).Name);
    }
}
