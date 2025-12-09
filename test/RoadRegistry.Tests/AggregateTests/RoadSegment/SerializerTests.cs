namespace RoadRegistry.Tests.AggregateTests.RoadSegment;

using System.Text;
using AutoFixture;
using FluentAssertions;
using Framework;
using Marten;
using Marten.Services;
using Newtonsoft.Json;
using NodaTime;
using NodaTime.Serialization.JsonNet;
using RoadRegistry.BackOffice;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using RoadRegistry.RoadSegment;
using RoadRegistry.RoadSegment.Events;
using RoadRegistry.RoadSegment.Events.V2;
using Weasel.Core;

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

        var aggregate = RoadSegment.Create(fixture.Create<RoadSegmentAdded>());

        var serializer = new StoreOptions().ConfigureSerializer().Serializer();
        var aggregateAsJson = serializer.ToJson(aggregate);
        var deserializedAggregate = serializer.FromJson<RoadSegment>(aggregateAsJson);

        _testOutputHelper.WriteLine($"Expected:\n{serializer.ToJson(aggregate)}");
        _testOutputHelper.WriteLine($"\nActual:\n{serializer.ToJson(deserializedAggregate)}");

        deserializedAggregate.Should().BeEquivalentTo(aggregate);
    }
}
