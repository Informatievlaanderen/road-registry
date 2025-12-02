namespace RoadRegistry.Tests.AggregateTests.RoadSegment;

using AutoFixture;
using FluentAssertions;
using Newtonsoft.Json;
using RoadRegistry.BackOffice;
using RoadRegistry.RoadSegment;
using RoadRegistry.RoadSegment.Events;

public class SerializerTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public SerializerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void CanSerializeAndDeserializeToJson()
    {
        var fixture = new RoadNetworkTestData().Fixture;

        var aggregate = RoadSegment.Create(fixture.Create<RoadSegmentAdded>());

        var jsonSerializerSettings = SqsJsonSerializerSettingsProvider.CreateSerializerSettings();
        var aggregateAsJson = JsonConvert.SerializeObject(aggregate, jsonSerializerSettings);
        var deserializedAggregate = JsonConvert.DeserializeObject<RoadSegment>(aggregateAsJson, jsonSerializerSettings);

        _testOutputHelper.WriteLine($"Expected:\n{JsonConvert.SerializeObject(aggregate, Formatting.Indented, jsonSerializerSettings)}");
        _testOutputHelper.WriteLine($"\nActual:\n{JsonConvert.SerializeObject(deserializedAggregate, Formatting.Indented, jsonSerializerSettings)}");

        deserializedAggregate.Should().BeEquivalentTo(aggregate);
    }
}
