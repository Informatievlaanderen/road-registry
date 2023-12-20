namespace RoadRegistry.Tests;

using Be.Vlaanderen.Basisregisters.EventHandling;
using Newtonsoft.Json;
using RoadRegistry.BackOffice.Messages;
using RoadRegistry.BackOffice.Uploads;

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
}
