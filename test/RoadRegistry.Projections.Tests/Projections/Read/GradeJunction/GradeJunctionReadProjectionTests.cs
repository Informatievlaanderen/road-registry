namespace RoadRegistry.Projections.Tests.Projections.ReadProjections.GradeJunction;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Microsoft.Extensions.Logging.Abstractions;
using RoadRegistry.GradeJunction.Events.V2;
using RoadRegistry.Read.Projections;
using RoadRegistry.Tests.AggregateTests;
using RoadRegistry.ValueObjects;

public class GradeJunctionReadProjectionTests
{
    private readonly RoadNetworkTestDataV2 _testData = new();

    private ProvenanceData Provenance => new(_testData.Provenance);

    private ReadProjectionScenario Scenario() => new(
        new RoadNodeReadProjection(),
        new RoadSegmentReadProjection(new FakeStreetNameClient(), NullLogger<RoadSegmentReadProjection>.Instance),
        new GradeJunctionReadProjection());

    private Task GivenRoadSegments1And2(ReadProjectionScenario scenario) => scenario.GivenAsync(
        _testData.Segment1StartNodeAdded, _testData.Segment1EndNodeAdded,
        _testData.Segment2StartNodeAdded, _testData.Segment2EndNodeAdded,
        _testData.Segment1Added, _testData.Segment2Added);

    private GradeJunctionWasAdded GradeJunction1Between(int roadSegmentId1, int roadSegmentId2) => new()
    {
        GradeJunctionId = new GradeJunctionId(1),
        RoadSegmentId1 = new RoadSegmentId(roadSegmentId1),
        RoadSegmentId2 = new RoadSegmentId(roadSegmentId2),
        Geometry = _testData.Fixture.Create<JunctionGeometry>(),
        Provenance = Provenance
    };

    private static async Task<GradeJunctionId[]> GradeJunctionIdsOf(ReadProjectionScenario scenario, int roadSegmentId)
    {
        var junctions = await scenario.Store.FindGradeJunctionsForRoadSegment(new RoadSegmentId(roadSegmentId), CancellationToken.None);
        return junctions.Select(x => x.GradeJunctionId).ToArray();
    }

    [Fact]
    public async Task WhenGradeJunctionWasAdded_ThenStoredAndFoundForBothRoadSegments()
    {
        var scenario = Scenario();
        await GivenRoadSegments1And2(scenario);

        await scenario.GivenAsync(GradeJunction1Between(1, 2));

        var junction = await scenario.Load<GradeJunctionReadItem>(1);
        Assert.NotNull(junction);
        Assert.True(junction!.IsV2);
        Assert.Equal(1, junction.RoadSegmentId1);
        Assert.Equal(2, junction.RoadSegmentId2);

        Assert.Contains(new GradeJunctionId(1), await GradeJunctionIdsOf(scenario, 1));
        Assert.Contains(new GradeJunctionId(1), await GradeJunctionIdsOf(scenario, 2));
    }

    // The crossing records the two segments it is between and nothing writes back to them, so it can be projected
    // before they exist. That is what lets the read projection take a correlation's events in any order.
    [Fact]
    public async Task WhenGradeJunctionIsProjectedBeforeItsRoadSegments_ThenStored()
    {
        var scenario = Scenario();

        await scenario.GivenAsync(GradeJunction1Between(1, 2));

        var junction = await scenario.Load<GradeJunctionReadItem>(1);
        Assert.NotNull(junction);
        Assert.Equal(1, junction!.RoadSegmentId1);
        Assert.Equal(2, junction.RoadSegmentId2);
    }

    [Fact]
    public async Task WhenGradeJunctionWasRemoved_ThenMarkedRemovedAndNoLongerFoundForItsRoadSegments()
    {
        var scenario = Scenario();
        await GivenRoadSegments1And2(scenario);
        await scenario.GivenAsync(GradeJunction1Between(1, 2));

        await scenario.GivenAsync(new GradeJunctionWasRemoved
        {
            GradeJunctionId = new GradeJunctionId(1),
            Provenance = Provenance
        });

        Assert.True((await scenario.Load<GradeJunctionReadItem>(1))!.IsRemoved);
        Assert.Empty(await GradeJunctionIdsOf(scenario, 1));
        Assert.Empty(await GradeJunctionIdsOf(scenario, 2));
    }

    [Fact]
    public async Task WhenGradeJunctionWasModified_ThenFoundForTheNewRoadSegmentOnly()
    {
        var scenario = Scenario();
        await GivenRoadSegments1And2(scenario);
        await scenario.GivenAsync(GradeJunction1Between(1, 2));

        await scenario.GivenAsync(new GradeJunctionWasModified
        {
            GradeJunctionId = new GradeJunctionId(1),
            RoadSegmentId2 = new RoadSegmentId(3),
            Provenance = Provenance
        });

        Assert.Contains(new GradeJunctionId(1), await GradeJunctionIdsOf(scenario, 1));
        Assert.Empty(await GradeJunctionIdsOf(scenario, 2));
        Assert.Contains(new GradeJunctionId(1), await GradeJunctionIdsOf(scenario, 3));
    }

    [Fact]
    public async Task WhenRemovingUnknownGradeJunction_ThenThrows()
    {
        var scenario = Scenario();

        await Assert.ThrowsAsync<InvalidOperationException>(() => scenario.GivenAsync(new GradeJunctionWasRemoved
        {
            GradeJunctionId = new GradeJunctionId(999),
            Provenance = Provenance
        }));
    }
}
