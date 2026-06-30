namespace RoadRegistry.Tests.AggregateTests.RoadNode;

using AutoFixture;
using FluentAssertions;
using Framework;
using Marten;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using RoadRegistry.RoadNode;
using RoadRegistry.RoadNode.Events.V2;

public class MartenSerializerTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public MartenSerializerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void AggregateCanSerializeAndDeserializeToJson()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;

        var original = RoadNode.Create(fixture.Create<RoadNodeWasAdded>());
        original.Apply(fixture.Create<RoadNodeTypeWasChanged>());

        var serializer = new StoreOptions().ConfigureSerializer().Serializer();
        var originalAsJson = serializer.ToJson(original);
        var deserialized = serializer.FromJson<RoadNode>(originalAsJson);

        _testOutputHelper.WriteLine($"Expected:\n{serializer.ToJson(original)}");
        _testOutputHelper.WriteLine($"\nActual:\n{serializer.ToJson(deserialized)}");

        deserialized.Should().BeEquivalentTo(original);
    }

    [Fact]
    public void LastEventHash_ReflectsAppliedEventHash()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;

        var evt = fixture.Create<RoadNodeWasAdded>();
        var node = RoadNode.Create(evt);

        node.LastEventHash.Should().Be(evt.GetHash());
    }

    [Fact]
    public void LastEventHash_IsPreservedThroughSerializationRoundtrip()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;

        var original = RoadNode.Create(fixture.Create<RoadNodeWasAdded>());
        var expectedHash = original.LastEventHash;

        var serializer = new StoreOptions().ConfigureSerializer().Serializer();
        var deserialized = serializer.FromJson<RoadNode>(serializer.ToJson(original));

        deserialized.LastEventHash.Should().Be(expectedHash);
    }

    [Fact]
    public void LastEventHash_IsPreservedWhenNoNewEventIsApplied()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;

        var original = RoadNode.Create(fixture.Create<RoadNodeWasAdded>());

        // Simulate loading from a snapshot: deserialize restores _lastSnapshotEventHash
        var serializer = new StoreOptions().ConfigureSerializer().Serializer();
        var loadedFromSnapshot = serializer.FromJson<RoadNode>(serializer.ToJson(original));

        // Without applying any new event, LastEventHash must still be the original
        loadedFromSnapshot.LastEventHash.Should().Be(original.LastEventHash);
    }
}
