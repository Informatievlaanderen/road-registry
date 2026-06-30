namespace RoadRegistry.Tests.AggregateTests.GradeSeparatedJunction;

using AutoFixture;
using FluentAssertions;
using Framework;
using Marten;
using RoadRegistry.GradeSeparatedJunction;
using RoadRegistry.GradeSeparatedJunction.Events.V2;
using RoadRegistry.Infrastructure.MartenDb.Setup;

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

        var original = GradeSeparatedJunction.Create(fixture.Create<GradeSeparatedJunctionWasAdded>());

        var serializer = new StoreOptions().ConfigureSerializer().Serializer();
        var originalAsJson = serializer.ToJson(original);
        var deserialized = serializer.FromJson<GradeSeparatedJunction>(originalAsJson);

        _testOutputHelper.WriteLine($"Expected:\n{serializer.ToJson(original)}");
        _testOutputHelper.WriteLine($"\nActual:\n{serializer.ToJson(deserialized)}");

        deserialized.Should().BeEquivalentTo(original);
    }

    [Fact]
    public void LastEventHash_ReflectsAppliedEventHash()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;

        var evt = fixture.Create<GradeSeparatedJunctionWasAdded>();
        var junction = GradeSeparatedJunction.Create(evt);

        junction.LastEventHash.Should().Be(evt.GetHash());
    }

    [Fact]
    public void LastEventHash_IsPreservedThroughSerializationRoundtrip()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;

        var original = GradeSeparatedJunction.Create(fixture.Create<GradeSeparatedJunctionWasAdded>());
        var expectedHash = original.LastEventHash;

        var serializer = new StoreOptions().ConfigureSerializer().Serializer();
        var deserialized = serializer.FromJson<GradeSeparatedJunction>(serializer.ToJson(original));

        deserialized.LastEventHash.Should().Be(expectedHash);
    }

    [Fact]
    public void LastEventHash_IsPreservedWhenNoNewEventIsApplied()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;

        var original = GradeSeparatedJunction.Create(fixture.Create<GradeSeparatedJunctionWasAdded>());

        // Simulate loading from a snapshot: deserialize restores _lastSnapshotEventHash
        var serializer = new StoreOptions().ConfigureSerializer().Serializer();
        var loadedFromSnapshot = serializer.FromJson<GradeSeparatedJunction>(serializer.ToJson(original));

        // Without applying any new event, LastEventHash must still be the original
        loadedFromSnapshot.LastEventHash.Should().Be(original.LastEventHash);
    }
}
