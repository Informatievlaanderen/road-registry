namespace RoadRegistry.Hosts.Infrastructure.Extensions;

using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

public static class ConfigurationBuilderExtensions
{
    public static ConfigurationBuilder UseDefaultConfiguration(this ConfigurationBuilder builder, IHostEnvironment environment, string[] args = null)
    {
        if (environment.IsProduction())
        {
            builder
                .SetBasePath(Directory.GetCurrentDirectory());
        }

        builder
            .AddJsonFile("appsettings.json", true, false)
            .AddJsonFile($"appsettings.{environment.EnvironmentName.ToLowerInvariant()}.json", true, false)
            .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", true, false)
            .AddEnvironmentVariables();

        if (args != null)
        {
            builder.AddCommandLine(args);
        }

        return builder;
    }
}
