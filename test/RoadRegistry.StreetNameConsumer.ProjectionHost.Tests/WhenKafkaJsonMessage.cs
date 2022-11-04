namespace RoadRegistry.StreetNameConsumer.ProjectionHost.Tests;

using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common;
using Be.Vlaanderen.Basisregisters.GrAr.Contracts.StreetNameRegistry;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;
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
        var message = new StreetNameWasRegistered("TEST_STREETNAME", "TEST_MUNICIPALITY", "DUMMY_NISCODE", new Provenance(DateTime.UtcNow.ToString(), "KAFKA TEST", "", "Digitaal Vlaanderen", "Testing serialization"));
        var serializedMessage = JsonConvert.SerializeObject(message, _serializerSettings);

        _outputHelper.WriteLine(serializedMessage);

        var kafkaJsonMessage = new KafkaJsonMessage(message.GetType().FullName ?? "Unknown", serializedMessage);
        var serializedKafkaMessage = JsonConvert.SerializeObject(kafkaJsonMessage, _serializerSettings);

        _outputHelper.WriteLine(serializedKafkaMessage);
    }
}
