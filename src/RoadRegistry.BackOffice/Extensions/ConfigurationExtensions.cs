namespace Microsoft.Extensions.Configuration;

using System.Configuration;

public static class ConfigurationExtensions
{
    public static T GetRequiredValue<T>(this IConfiguration configuration, string key)
    {
        return configuration.GetValue<T>(key) ?? throw new ConfigurationErrorsException($"The '{key}' configuration variable was not set.");
    }

    public static T GetSection<T>(this IConfiguration configuration, string key)
        where T: new()
    {
        var section = new T();
        configuration.GetSection(key).Bind(section);
        return section;
    }
}
