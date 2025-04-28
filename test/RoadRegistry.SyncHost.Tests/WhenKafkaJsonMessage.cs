namespace RoadRegistry.SyncHost.Tests;

using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common;
using Be.Vlaanderen.Basisregisters.GrAr.Contracts.StreetNameRegistry;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
using Newtonsoft.Json;
using Xunit.Abstractions;

public class WhenKafkaJsonMessage
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly JsonSerializerSettings _serializerSettings;

    public WhenKafkaJsonMessage(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        _serializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
    }

    [Fact]
    public void ItShouldSerialize()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-mm-dd HH:mm:ss");
        var message = new StreetNameWasRegistered("TEST_STREETNAME", "TEST_MUNICIPALITY", "DUMMY_NISCODE", new Provenance(timestamp, "KAFKA TEST", "", "Digitaal Vlaanderen", "Testing serialization"));
        var serializedMessage = JsonConvert.SerializeObject(message, _serializerSettings);

        _outputHelper.WriteLine(serializedMessage);

        var kafkaJsonMessage = new JsonMessage(message.GetType().FullName ?? "Unknown", serializedMessage);
        var serializedKafkaMessage = JsonConvert.SerializeObject(kafkaJsonMessage, _serializerSettings);

        _outputHelper.WriteLine(serializedKafkaMessage);

        var expected = "{\"type\":\"Be.Vlaanderen.Basisregisters.GrAr.Contracts.StreetNameRegistry.StreetNameWasRegistered\",\"data\":\"{\\\"streetNameId\\\":\\\"TEST_STREETNAME\\\",\\\"municipalityId\\\":\\\"TEST_MUNICIPALITY\\\",\\\"nisCode\\\":\\\"DUMMY_NISCODE\\\",\\\"provenance\\\":{\\\"timestamp\\\":\\\"" + timestamp + "\\\",\\\"application\\\":\\\"KAFKA TEST\\\",\\\"modification\\\":\\\"\\\",\\\"organisation\\\":\\\"Digitaal Vlaanderen\\\",\\\"reason\\\":\\\"Testing serialization\\\"}}\"}";
        Assert.Equal(expected, serializedKafkaMessage);
    }
}
