namespace RoadRegistry.Tests;

using Be.Vlaanderen.Basisregisters.EventHandling;
using Newtonsoft.Json;
using RoadNetwork;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Messages;
using RoadRegistry.BackOffice.Uploads;
using ScopedRoadNetwork;

public static class UploadsExtensions
{
    public static string Describe(this TranslatedChanges changes, Action<RequestedChange> modifyChange = null)
    {
        var requestedChanges = changes.Select(change =>
        {
            var requestedChange = new RequestedChange();
            change.TranslateTo(requestedChange);

            modifyChange?.Invoke(requestedChange);

            return requestedChange;
        }).ToList();
        return JsonConvert.SerializeObject(requestedChanges, Formatting.Indented, EventsJsonSerializerSettingsProvider.CreateSerializerSettings());
    }

    public static string Describe(this Extracts.FeatureCompare.DomainV2.TranslatedChanges changes, Func<IRoadNetworkChange, IRoadNetworkChange> modifyChange = null)
    {
        var requestedChanges = changes
            .Select(change => modifyChange?.Invoke(change) ?? change)
            .ToList();

        var jsonSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
        foreach (var converter in WellKnownJsonConverters.Converters)
        {
            jsonSettings.Converters.Add(converter);
        }

        return JsonConvert.SerializeObject(requestedChanges, Formatting.Indented, jsonSettings);
    }
}
