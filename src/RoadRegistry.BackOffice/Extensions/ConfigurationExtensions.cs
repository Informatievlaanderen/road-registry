namespace Microsoft.Extensions.Configuration;

using System.Configuration;
using RoadRegistry.BackOffice;

public static class ConfigurationExtensions
{
    public static T GetRequiredValue<T>(this IConfiguration configuration, string key)
    {
        return configuration.GetValue<T>(key) ?? throw new ConfigurationErrorsException($"The '{key}' configuration variable was not set.");
    }

    public static TOptions GetOptions<TOptions>(this IConfiguration configuration)
        where TOptions : class, new()
    {
        var configurationSectionName = new TOptions() is IHasConfigurationKey hasConfigurationKey ? hasConfigurationKey.GetConfigurationKey() : null;
        return GetOptions<TOptions>(configuration, configurationSectionName);
    }

    public static TOptions GetOptions<TOptions>(this IConfiguration configuration, string configurationSectionKey)
        where TOptions : class, new()
    {
        var options = new TOptions();
        if (configurationSectionKey != null)
        {
            configuration.GetSection(configurationSectionKey).Bind(options, ConfigureBinder);
        }
        else
        {
            configuration.Bind(options, ConfigureBinder);
        }

        return options;
    }

    private static void ConfigureBinder(BinderOptions binderOptions)
    {
        binderOptions.BindNonPublicProperties = true;
    }
}
