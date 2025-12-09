namespace RoadRegistry.Tests.AggregateTests.GradeSeparatedJunction;

using AutoFixture;
using FluentAssertions;
using Framework;
using Marten;
using Newtonsoft.Json;
using RoadRegistry.BackOffice;
using RoadRegistry.GradeSeparatedJunction;
using RoadRegistry.GradeSeparatedJunction.Events;
using RoadRegistry.GradeSeparatedJunction.Events.V2;
using RoadRegistry.Infrastructure.MartenDb.Setup;

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

        var aggregate = GradeSeparatedJunction.Create(fixture.Create<GradeSeparatedJunctionAdded>());

        var serializer = new StoreOptions().ConfigureSerializer().Serializer();
        var aggregateAsJson = serializer.ToJson(aggregate);
        var deserializedAggregate = serializer.FromJson<GradeSeparatedJunction>(aggregateAsJson);

        _testOutputHelper.WriteLine($"Expected:\n{serializer.ToJson(aggregate)}");
        _testOutputHelper.WriteLine($"\nActual:\n{serializer.ToJson(deserializedAggregate)}");

        deserializedAggregate.Should().BeEquivalentTo(aggregate);
    }
}
