namespace RoadRegistry.BackOffice.MessagingHost.Sqs.Tests;

using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class JObjectTests
{
    [Fact]
    public void TestNew()
    {
        var serializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
        var @event = JsonConvert.SerializeObject(new UploadRoadNetworkChangesArchive { ArchiveId = Guid.NewGuid().ToString("N") }, serializerSettings);
        var sqsJsonMessage = new SqsJsonMessage("RoadRegistry.Messages.UploadRoadNetworkChangesArchive", @event);

        var deserialized = (JObject)sqsJsonMessage.Map();
        var typedObject = deserialized.ToObject(typeof(UploadRoadNetworkChangesArchive));
    }
}