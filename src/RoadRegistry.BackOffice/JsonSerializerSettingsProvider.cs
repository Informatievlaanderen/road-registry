namespace RoadRegistry.BackOffice;

using Be.Vlaanderen.Basisregisters.EventHandling;
using Infrastructure.Converters;
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

        settings.Converters.Add(new OrganizationIdConverter());
        settings.Converters.Add(new RoadSegmentIdConverter());
        settings.Converters.Add(new RoadSegmentAccessRestrictionConverter());
        settings.Converters.Add(new RoadSegmentCategoryConverter());
        settings.Converters.Add(new RoadSegmentLaneCountConverter());
        settings.Converters.Add(new RoadSegmentLaneDirectionConverter());
        settings.Converters.Add(new RoadSegmentMorphologyConverter());
        settings.Converters.Add(new RoadSegmentPositionConverter());
        settings.Converters.Add(new RoadSegmentStatusConverter());
        settings.Converters.Add(new RoadSegmentSurfaceTypeConverter());
        settings.Converters.Add(new RoadSegmentWidthConverter());
        settings.Converters.Add(new EuropeanRoadNumberConverter());
        settings.Converters.Add(new NationalRoadNumberConverter());
        settings.Converters.Add(new NumberedRoadNumberConverter());
        settings.Converters.Add(new RoadSegmentNumberedRoadDirectionConverter());
        settings.Converters.Add(new RoadSegmentNumberedRoadOrdinalConverter());

        foreach (var converter in WellKnownJsonConverters.Converters)
        {
            settings.Converters.Add(converter);

        }

        return settings;
    }
}
