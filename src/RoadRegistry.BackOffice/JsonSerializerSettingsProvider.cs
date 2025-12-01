namespace RoadRegistry.BackOffice;

using Be.Vlaanderen.Basisregisters.EventHandling;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public static class SqsJsonSerializerSettingsProvider
{
    public static JsonSerializerSettings CreateSerializerSettings()
    {
        var settings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

        settings.ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy
            {
                OverrideSpecifiedNames = true,
                ProcessDictionaryKeys = false,
                ProcessExtensionDataNames = true
            }
        };

        settings.NullValueHandling = NullValueHandling.Ignore;
        settings.Formatting = Formatting.None;

        foreach (var converter in WellKnownJsonConverters.Converters)
        {
            settings.Converters.Add(converter);
        }

        return settings;
    }
}
