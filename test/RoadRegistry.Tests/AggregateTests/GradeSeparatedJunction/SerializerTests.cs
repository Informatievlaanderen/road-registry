namespace RoadRegistry.Tests.AggregateTests.GradeSeparatedJunction;

using AutoFixture;
using FluentAssertions;
using Newtonsoft.Json;
using RoadRegistry.BackOffice;
using RoadRegistry.GradeSeparatedJunction;
using RoadRegistry.GradeSeparatedJunction.Events;

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

        var aggregate = GradeSeparatedJunction.Create(fixture.Create<GradeSeparatedJunctionAdded>());

        var jsonSerializerSettings = SqsJsonSerializerSettingsProvider.CreateSerializerSettings();
        var aggregateAsJson = JsonConvert.SerializeObject(aggregate, jsonSerializerSettings);
        var deserializedAggregate = JsonConvert.DeserializeObject<GradeSeparatedJunction>(aggregateAsJson, jsonSerializerSettings);

        _testOutputHelper.WriteLine($"Expected:\n{JsonConvert.SerializeObject(aggregate, Formatting.Indented, jsonSerializerSettings)}");
        _testOutputHelper.WriteLine($"\nActual:\n{JsonConvert.SerializeObject(deserializedAggregate, Formatting.Indented, jsonSerializerSettings)}");

        deserializedAggregate.Should().BeEquivalentTo(aggregate);
    }
}
