namespace RoadRegistry;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NodaTime;
using NodaTime.Serialization.JsonNet;

public static class WellKnownJsonSerializerSettings
{
    public static readonly JsonSerializerSettings Marten = new JsonSerializerSettings().ConfigureForMarten();

    public static JsonSerializerSettings ConfigureForMarten(this JsonSerializerSettings settings)
    {
        settings.MaxDepth = 32;
        settings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
        settings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
        settings
            .ConfigureForNodaTime(DateTimeZoneProviders.Tzdb)
            .WithIsoIntervalConverter();

        // Do not change this setting
        // Setting this to None prevents Json.NET from loading malicious, unsafe, or security-sensitive types
        settings.TypeNameHandling = TypeNameHandling.None;

        foreach (var converter in WellKnownJsonConverters.Converters)
        {
            settings.Converters.Add(converter);
        }

        settings.ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy
            {
                OverrideSpecifiedNames = true,
                ProcessDictionaryKeys = false,
                ProcessExtensionDataNames = true
            }
        };

        settings.NullValueHandling = NullValueHandling.Include;
        settings.Formatting = Formatting.None;

        return settings;
    }
}
