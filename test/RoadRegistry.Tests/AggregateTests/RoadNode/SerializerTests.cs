namespace RoadRegistry.Tests.AggregateTests.RoadNode;

using AutoFixture;
using FluentAssertions;
using Framework;
using Marten;
using Newtonsoft.Json;
using RoadRegistry.BackOffice;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using RoadRegistry.RoadNode;
using RoadRegistry.RoadNode.Events;
using RoadRegistry.RoadNode.Events.V2;

public class SerializerTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public SerializerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void AggregateCanSerializeAndDeserializeToJson()
    {
        var fixture = new RoadNetworkTestData().Fixture;

        var aggregate = RoadNode.Create(fixture.Create<RoadNodeAdded>());

        var serializer = new StoreOptions().ConfigureSerializer().Serializer();
        var aggregateAsJson = serializer.ToJson(aggregate);
        var deserializedAggregate = serializer.FromJson<RoadNode>(aggregateAsJson);

        _testOutputHelper.WriteLine($"Expected:\n{serializer.ToJson(aggregate)}");
        _testOutputHelper.WriteLine($"\nActual:\n{serializer.ToJson(deserializedAggregate)}");

        deserializedAggregate.Should().BeEquivalentTo(aggregate);
    }
}
