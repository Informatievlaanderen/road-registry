namespace RoadRegistry.Tests.AggregateTests.GradeJunction;

using AutoFixture;
using FluentAssertions;
using Marten;
using RoadRegistry.GradeJunction;
using RoadRegistry.GradeJunction.Events.V2;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using RoadRegistry.Tests.AggregateTests.Framework;

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

        var original = GradeJunction.Create(fixture.Create<GradeJunctionWasAdded>());

        var serializer = new StoreOptions().ConfigureSerializer().Serializer();
        var originalAsJson = serializer.ToJson(original);
        var deserialized = serializer.FromJson<GradeJunction>(originalAsJson);

        _testOutputHelper.WriteLine($"Expected:\n{serializer.ToJson(original)}");
        _testOutputHelper.WriteLine($"\nActual:\n{serializer.ToJson(deserialized)}");

        deserialized.Should().BeEquivalentTo(original);
    }

    [Fact]
    public void LastEventHash_ReflectsAppliedEventHash()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;

        var evt = fixture.Create<GradeJunctionWasAdded>();
        var junction = GradeJunction.Create(evt);

        junction.LastEventHash.Should().Be(evt.GetHash());
    }

    [Fact]
    public void LastEventHash_IsPreservedThroughSerializationRoundtrip()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;

        var original = GradeJunction.Create(fixture.Create<GradeJunctionWasAdded>());
        var expectedHash = original.LastEventHash;

        var serializer = new StoreOptions().ConfigureSerializer().Serializer();
        var deserialized = serializer.FromJson<GradeJunction>(serializer.ToJson(original));

        deserialized.LastEventHash.Should().Be(expectedHash);
    }

    [Fact]
    public void LastEventHash_IsPreservedWhenNoNewEventIsApplied()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;

        var original = GradeJunction.Create(fixture.Create<GradeJunctionWasAdded>());

        // Simulate loading from a snapshot: deserialize restores _lastSnapshotEventHash
        var serializer = new StoreOptions().ConfigureSerializer().Serializer();
        var loadedFromSnapshot = serializer.FromJson<GradeJunction>(serializer.ToJson(original));

        // Without applying any new event, LastEventHash must still be the original
        loadedFromSnapshot.LastEventHash.Should().Be(original.LastEventHash);
    }
}
