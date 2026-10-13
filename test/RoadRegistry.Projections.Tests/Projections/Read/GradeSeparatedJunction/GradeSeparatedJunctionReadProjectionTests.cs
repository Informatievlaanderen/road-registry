namespace RoadRegistry.Projections.Tests.Projections.ReadProjections.GradeSeparatedJunction;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Microsoft.Extensions.Logging.Abstractions;
using RoadRegistry.GradeSeparatedJunction.Events.V2;
using RoadRegistry.Read.Projections;
using RoadRegistry.Tests.AggregateTests;
using RoadRegistry.ValueObjects;
using V1 = RoadRegistry.GradeSeparatedJunction.Events.V1;

public class GradeSeparatedJunctionReadProjectionTests
{
    private readonly RoadNetworkTestDataV2 _testData = new();

    private ProvenanceData Provenance => new(_testData.Provenance);

    private ReadProjectionScenario Scenario() => new(
        new RoadNodeReadProjection(),
        new RoadSegmentReadProjection(new FakeStreetNameClient(), NullLogger<RoadSegmentReadProjection>.Instance),
        new GradeSeparatedJunctionReadProjection());

    private Task GivenRoadSegments1To3(ReadProjectionScenario scenario) => scenario.GivenAsync(
        _testData.Segment1StartNodeAdded, _testData.Segment1EndNodeAdded,
        _testData.Segment2StartNodeAdded, _testData.Segment2EndNodeAdded,
        _testData.Segment3StartNodeAdded, _testData.Segment3EndNodeAdded,
        _testData.Segment1Added, _testData.Segment2Added, _testData.Segment3Added);

    private static async Task<GradeSeparatedJunctionId[]> GradeSeparatedJunctionIdsOf(ReadProjectionScenario scenario, int roadSegmentId)
    {
        var junctions = await scenario.Store.FindGradeSeparatedJunctionsForRoadSegment(new RoadSegmentId(roadSegmentId), CancellationToken.None);
        return junctions.Select(x => x.GradeSeparatedJunctionId).ToArray();
    }

    [Fact]
    public async Task WhenV1GradeSeparatedJunctionAdded_ThenStoredAndFoundForBothRoadSegments()
    {
        var scenario = Scenario();
        await GivenRoadSegments1To3(scenario);

        await scenario.GivenAsync(new V1.GradeSeparatedJunctionAdded
        {
            Id = 1,
            TemporaryId = -1,
            LowerRoadSegmentId = 1,
            UpperRoadSegmentId = 2,
            Type = "GelijkgrondseKruising",
            Provenance = Provenance
        });

        var junction = await scenario.Load<GradeSeparatedJunctionReadItem>(1);
        Assert.NotNull(junction);
        Assert.False(junction!.IsV2);

        Assert.Contains(new GradeSeparatedJunctionId(1), await GradeSeparatedJunctionIdsOf(scenario, 1));
        Assert.Contains(new GradeSeparatedJunctionId(1), await GradeSeparatedJunctionIdsOf(scenario, 2));
    }

    [Fact]
    public async Task WhenV1GradeSeparatedJunctionModified_ThenFoundForTheNewRoadSegmentOnly()
    {
        var scenario = Scenario();
        await GivenRoadSegments1To3(scenario);
        await scenario.GivenAsync(new V1.GradeSeparatedJunctionAdded
        {
            Id = 1,
            TemporaryId = -1,
            LowerRoadSegmentId = 1,
            UpperRoadSegmentId = 2,
            Type = "GelijkgrondseKruising",
            Provenance = Provenance
        });

        await scenario.GivenAsync(new V1.GradeSeparatedJunctionModified
        {
            Id = 1,
            LowerRoadSegmentId = 3,
            UpperRoadSegmentId = 2,
            Type = "Tunnel",
            Provenance = Provenance
        });

        Assert.Equal("Tunnel", (await scenario.Load<GradeSeparatedJunctionReadItem>(1))!.Type);
        Assert.Empty(await GradeSeparatedJunctionIdsOf(scenario, 1));
        Assert.Contains(new GradeSeparatedJunctionId(1), await GradeSeparatedJunctionIdsOf(scenario, 3));
        Assert.Contains(new GradeSeparatedJunctionId(1), await GradeSeparatedJunctionIdsOf(scenario, 2));
    }

    [Fact]
    public async Task WhenV1GradeSeparatedJunctionRemoved_ThenMarkedRemovedAndNoLongerFoundForItsRoadSegments()
    {
        var scenario = Scenario();
        await GivenRoadSegments1To3(scenario);
        await scenario.GivenAsync(new V1.GradeSeparatedJunctionAdded
        {
            Id = 1,
            TemporaryId = -1,
            LowerRoadSegmentId = 1,
            UpperRoadSegmentId = 2,
            Type = "GelijkgrondseKruising",
            Provenance = Provenance
        });

        await scenario.GivenAsync(new V1.GradeSeparatedJunctionRemoved { Id = 1, Provenance = Provenance });

        Assert.True((await scenario.Load<GradeSeparatedJunctionReadItem>(1))!.IsRemoved);
        Assert.Empty(await GradeSeparatedJunctionIdsOf(scenario, 1));
        Assert.Empty(await GradeSeparatedJunctionIdsOf(scenario, 2));
    }

    [Fact]
    public async Task WhenGradeSeparatedJunctionWasAdded_ThenStoredAndFoundForBothRoadSegments()
    {
        var scenario = Scenario();
        var type = _testData.Fixture.Create<GradeSeparatedJunctionTypeV2>();
        await GivenRoadSegments1To3(scenario);

        await scenario.GivenAsync(new GradeSeparatedJunctionWasAdded
        {
            GradeSeparatedJunctionId = new GradeSeparatedJunctionId(1),
            LowerRoadSegmentId = new RoadSegmentId(1),
            UpperRoadSegmentId = new RoadSegmentId(2),
            Type = type,
            Geometry = _testData.Fixture.Create<JunctionGeometry>(),
            Provenance = Provenance
        });

        var junction = await scenario.Load<GradeSeparatedJunctionReadItem>(1);
        Assert.Equal(type.ToString(), junction!.Type);
        Assert.True(junction.IsV2);

        Assert.Contains(new GradeSeparatedJunctionId(1), await GradeSeparatedJunctionIdsOf(scenario, 1));
        Assert.Contains(new GradeSeparatedJunctionId(1), await GradeSeparatedJunctionIdsOf(scenario, 2));
    }

    // The crossing records the two segments it is between and nothing writes back to them, so it can be projected
    // before they exist. That is what lets the read projection take a correlation's events in any order.
    [Fact]
    public async Task WhenGradeSeparatedJunctionIsProjectedBeforeItsRoadSegments_ThenStored()
    {
        var scenario = Scenario();

        await scenario.GivenAsync(new GradeSeparatedJunctionWasAdded
        {
            GradeSeparatedJunctionId = new GradeSeparatedJunctionId(1),
            LowerRoadSegmentId = new RoadSegmentId(1),
            UpperRoadSegmentId = new RoadSegmentId(2),
            Type = _testData.Fixture.Create<GradeSeparatedJunctionTypeV2>(),
            Geometry = _testData.Fixture.Create<JunctionGeometry>(),
            Provenance = Provenance
        });

        var junction = await scenario.Load<GradeSeparatedJunctionReadItem>(1);
        Assert.NotNull(junction);
        Assert.Equal(1, junction!.LowerRoadSegmentId);
        Assert.Equal(2, junction.UpperRoadSegmentId);
    }

    [Fact]
    public async Task WhenRemovingUnknownGradeSeparatedJunction_ThenThrows()
    {
        var scenario = Scenario();

        await Assert.ThrowsAsync<InvalidOperationException>(() => scenario.GivenAsync(new GradeSeparatedJunctionWasRemoved
        {
            GradeSeparatedJunctionId = new GradeSeparatedJunctionId(999),
            Provenance = Provenance
        }));
    }

    [Fact]
    public async Task WhenGradeSeparatedJunctionWasModified_WithNullFields_ThenKeepsExistingValues()
    {
        var scenario = Scenario();
        var type = _testData.Fixture.Create<GradeSeparatedJunctionTypeV2>();
        await GivenRoadSegments1To3(scenario);
        await scenario.GivenAsync(new GradeSeparatedJunctionWasAdded
        {
            GradeSeparatedJunctionId = new GradeSeparatedJunctionId(1),
            LowerRoadSegmentId = new RoadSegmentId(1),
            UpperRoadSegmentId = new RoadSegmentId(2),
            Type = type,
            Geometry = _testData.Fixture.Create<JunctionGeometry>(),
            Provenance = Provenance
        });

        await scenario.GivenAsync(new GradeSeparatedJunctionWasModified
        {
            GradeSeparatedJunctionId = new GradeSeparatedJunctionId(1),
            LowerRoadSegmentId = null,
            UpperRoadSegmentId = null,
            Type = null,
            Provenance = Provenance
        });

        var junction = await scenario.Load<GradeSeparatedJunctionReadItem>(1);
        Assert.Equal(type.ToString(), junction!.Type);
        Assert.Equal(1, junction.LowerRoadSegmentId);
        Assert.Contains(new GradeSeparatedJunctionId(1), await GradeSeparatedJunctionIdsOf(scenario, 1));
        Assert.Contains(new GradeSeparatedJunctionId(1), await GradeSeparatedJunctionIdsOf(scenario, 2));
    }

    [Fact]
    public async Task WhenV1GradeSeparatedJunctionWasMigrated_ThenV2AndFoundForTheNewRoadSegmentOnly()
    {
        var scenario = Scenario();
        var type = _testData.Fixture.Create<GradeSeparatedJunctionTypeV2>();
        await GivenRoadSegments1To3(scenario);
        await scenario.GivenAsync(new V1.GradeSeparatedJunctionAdded
        {
            Id = 1,
            TemporaryId = -1,
            LowerRoadSegmentId = 1,
            UpperRoadSegmentId = 2,
            Type = "Tunnel",
            Provenance = Provenance
        });

        await scenario.GivenAsync(new GradeSeparatedJunctionWasMigrated
        {
            GradeSeparatedJunctionId = new GradeSeparatedJunctionId(1),
            LowerRoadSegmentId = new RoadSegmentId(1),
            UpperRoadSegmentId = new RoadSegmentId(3),
            Type = type,
            Provenance = Provenance
        });

        var junction = await scenario.Load<GradeSeparatedJunctionReadItem>(1);
        Assert.True(junction!.IsV2);
        Assert.Equal(type.ToString(), junction.Type);
        Assert.Equal(1, junction.LowerRoadSegmentId);
        Assert.Equal(3, junction.UpperRoadSegmentId);
        Assert.Contains(new GradeSeparatedJunctionId(1), await GradeSeparatedJunctionIdsOf(scenario, 1));
        Assert.Empty(await GradeSeparatedJunctionIdsOf(scenario, 2));
        Assert.Contains(new GradeSeparatedJunctionId(1), await GradeSeparatedJunctionIdsOf(scenario, 3));
    }

    [Fact]
    public async Task WhenGradeSeparatedJunctionWasRemoved_ThenMarkedRemovedAndNoLongerFoundForItsRoadSegments()
    {
        var scenario = Scenario();
        var type = _testData.Fixture.Create<GradeSeparatedJunctionTypeV2>();
        await GivenRoadSegments1To3(scenario);
        await scenario.GivenAsync(new GradeSeparatedJunctionWasAdded
        {
            GradeSeparatedJunctionId = new GradeSeparatedJunctionId(1),
            LowerRoadSegmentId = new RoadSegmentId(1),
            UpperRoadSegmentId = new RoadSegmentId(2),
            Type = type,
            Geometry = _testData.Fixture.Create<JunctionGeometry>(),
            Provenance = Provenance
        });

        await scenario.GivenAsync(new GradeSeparatedJunctionWasRemoved
        {
            GradeSeparatedJunctionId = new GradeSeparatedJunctionId(1),
            Provenance = Provenance
        });

        Assert.True((await scenario.Load<GradeSeparatedJunctionReadItem>(1))!.IsRemoved);
        Assert.Empty(await GradeSeparatedJunctionIdsOf(scenario, 1));
        Assert.Empty(await GradeSeparatedJunctionIdsOf(scenario, 2));
    }
}
