namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.RoadNetwork.WhenRemovingRoadSegments;

using AutoFixture;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using FluentAssertions;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using Sqs.RoadNetwork;
using Tests.AggregateTests;
using Xunit.Abstractions;

public class SqsSerializerTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public SqsSerializerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void CanSerializeAndDeserializeToJson()
    {
        var fixture = new RoadNetworkTestData().Fixture;

        var original = fixture.Create<RemoveRoadSegmentsSqsRequest>();

        var jsonSerializerSettings = SqsJsonSerializerSettingsProvider.CreateSerializerSettings();
        var originalAsJson = JsonConvert.SerializeObject(original, jsonSerializerSettings);
        var deserialized = JsonConvert.DeserializeObject<RemoveRoadSegmentsSqsRequest>(originalAsJson, jsonSerializerSettings);

        _testOutputHelper.WriteLine($"Expected:\n{JsonConvert.SerializeObject(original, Formatting.Indented, jsonSerializerSettings)}");
        _testOutputHelper.WriteLine($"\nActual:\n{JsonConvert.SerializeObject(deserialized, Formatting.Indented, jsonSerializerSettings)}");

        deserialized.Should().BeEquivalentTo(original);
    }
}
