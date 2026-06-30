namespace RoadRegistry.Tests.AggregateTests.RoadSegment;

using AutoFixture;
using FluentAssertions;
using Framework;
using Marten;
using RoadRegistry.Extensions;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using RoadRegistry.RoadSegment;
using RoadRegistry.RoadSegment.Events.V2;

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

        var evt = fixture.Create<RoadSegmentWasAdded>();
        var original = RoadSegment.Create(evt with
        {
            Geometry = evt.Geometry.EnsureLambert08()
        });

        var serializer = new StoreOptions().ConfigureSerializer().Serializer();
        var originalAsJson = serializer.ToJson(original);
        var deserialized = serializer.FromJson<RoadSegment>(originalAsJson);

        _testOutputHelper.WriteLine($"Expected:\n{serializer.ToJson(original)}");
        _testOutputHelper.WriteLine($"\nActual:\n{serializer.ToJson(deserialized)}");

        deserialized.Should().BeEquivalentTo(original);
    }

    [Fact]
    public void LastEventHash_ReflectsAppliedEventHash()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;

        var rawEvt = fixture.Create<RoadSegmentWasAdded>();
        var appliedEvt = rawEvt with { Geometry = rawEvt.Geometry.EnsureLambert08() };
        var segment = RoadSegment.Create(appliedEvt);

        segment.LastEventHash.Should().Be(appliedEvt.GetHash());
    }

    [Fact]
    public void LastEventHash_IsPreservedThroughSerializationRoundtrip()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;

        var rawEvt = fixture.Create<RoadSegmentWasAdded>();
        var original = RoadSegment.Create(rawEvt with { Geometry = rawEvt.Geometry.EnsureLambert08() });
        var expectedHash = original.LastEventHash;

        var serializer = new StoreOptions().ConfigureSerializer().Serializer();
        var deserialized = serializer.FromJson<RoadSegment>(serializer.ToJson(original));

        deserialized.LastEventHash.Should().Be(expectedHash);
    }

    [Fact]
    public void LastEventHash_IsPreservedWhenNoNewEventIsApplied()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;

        var rawEvt = fixture.Create<RoadSegmentWasAdded>();
        var original = RoadSegment.Create(rawEvt with { Geometry = rawEvt.Geometry.EnsureLambert08() });

        // Simulate loading from a snapshot: deserialize restores _lastSnapshotEventHash
        var serializer = new StoreOptions().ConfigureSerializer().Serializer();
        var loadedFromSnapshot = serializer.FromJson<RoadSegment>(serializer.ToJson(original));

        // Without applying any new event, LastEventHash must still be the original
        loadedFromSnapshot.LastEventHash.Should().Be(original.LastEventHash);
    }

    [Fact]
    public void Outlined_AggregateCanSerializeAndDeserializeToJson()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;

        var rawEvt = fixture.Create<OutlinedRoadSegmentWasAdded>();
        var original = RoadSegment.Create(rawEvt with { Geometry = rawEvt.Geometry.EnsureLambert08() });

        var serializer = new StoreOptions().ConfigureSerializer().Serializer();
        var originalAsJson = serializer.ToJson(original);
        var deserialized = serializer.FromJson<RoadSegment>(originalAsJson);

        _testOutputHelper.WriteLine($"Expected:\n{serializer.ToJson(original)}");
        _testOutputHelper.WriteLine($"\nActual:\n{serializer.ToJson(deserialized)}");

        deserialized.Should().BeEquivalentTo(original);
    }

    [Fact]
    public void Outlined_LastEventHash_ReflectsAppliedEventHash()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;

        var evt = fixture.Create<OutlinedRoadSegmentWasAdded>();
        var segment = RoadSegment.Create(evt);

        segment.LastEventHash.Should().Be(evt.GetHash());
    }

    [Fact]
    public void Outlined_LastEventHash_IsPreservedThroughSerializationRoundtrip()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;

        var rawEvt = fixture.Create<OutlinedRoadSegmentWasAdded>();
        var original = RoadSegment.Create(rawEvt with { Geometry = rawEvt.Geometry.EnsureLambert08() });
        var expectedHash = original.LastEventHash;

        var serializer = new StoreOptions().ConfigureSerializer().Serializer();
        var deserialized = serializer.FromJson<RoadSegment>(serializer.ToJson(original));

        deserialized.LastEventHash.Should().Be(expectedHash);
    }

    [Fact]
    public void Outlined_LastEventHash_IsPreservedWhenNoNewEventIsApplied()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;

        var rawEvt = fixture.Create<OutlinedRoadSegmentWasAdded>();
        var original = RoadSegment.Create(rawEvt with { Geometry = rawEvt.Geometry.EnsureLambert08() });

        // Simulate loading from a snapshot: deserialize restores _lastSnapshotEventHash
        var serializer = new StoreOptions().ConfigureSerializer().Serializer();
        var loadedFromSnapshot = serializer.FromJson<RoadSegment>(serializer.ToJson(original));

        // Without applying any new event, LastEventHash must still be the original
        loadedFromSnapshot.LastEventHash.Should().Be(original.LastEventHash);
    }
}
