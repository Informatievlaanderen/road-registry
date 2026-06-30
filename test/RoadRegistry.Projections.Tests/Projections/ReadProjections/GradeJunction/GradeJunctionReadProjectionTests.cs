namespace RoadRegistry.Projections.Tests.Projections.ReadProjections;

using System;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Microsoft.Extensions.Logging.Abstractions;
using RoadRegistry.GradeJunction.Events.V2;
using RoadRegistry.Read.Projections;
using RoadRegistry.Tests;
using RoadRegistry.Tests.AggregateTests;

public class GradeJunctionReadProjectionTests
{
    private readonly RoadNetworkTestDataV2 _testData = new();

    private ProvenanceData Provenance => new(_testData.Provenance);

    private ReadProjectionScenario Scenario() => new(
        new RoadNodeReadProjection(),
        new RoadSegmentReadProjection(new FakeStreetNameClient(), NullLogger<RoadSegmentReadProjection>.Instance),
        new GradeJunctionReadProjection());

    // Grade junctions reference existing road segments, so the referenced segments (and their nodes) must be projected first.
    private Task GivenRoadSegments1And2(ReadProjectionScenario scenario) => scenario.GivenAsync(
        _testData.Segment1StartNodeAdded, _testData.Segment1EndNodeAdded,
        _testData.Segment2StartNodeAdded, _testData.Segment2EndNodeAdded,
        _testData.Segment1Added, _testData.Segment2Added);

    [Fact]
    public async Task WhenGradeJunctionWasAdded_ThenStoredAndRoadSegmentsReferenceTheJunction()
    {
        var scenario = Scenario();
        await GivenRoadSegments1And2(scenario);

        await scenario.GivenAsync(new GradeJunctionWasAdded
        {
            GradeJunctionId = new GradeJunctionId(1),
            RoadSegmentId1 = new RoadSegmentId(1),
            RoadSegmentId2 = new RoadSegmentId(2),
            Provenance = Provenance
        });

        var junction = await scenario.Load<GradeJunctionReadItem>(1);
        Assert.NotNull(junction);
        Assert.True(junction!.IsV2);

        Assert.Contains(new GradeJunctionId(1), (await scenario.Load<RoadSegmentReadItem>(1))!.GradeJunctionIds);
        Assert.Contains(new GradeJunctionId(1), (await scenario.Load<RoadSegmentReadItem>(2))!.GradeJunctionIds);
    }

    [Fact]
    public async Task WhenGradeJunctionWasRemoved_ThenMarkedRemovedAndRoadSegmentsNoLongerReferenceIt()
    {
        var scenario = Scenario();
        await GivenRoadSegments1And2(scenario);
        await scenario.GivenAsync(new GradeJunctionWasAdded
        {
            GradeJunctionId = new GradeJunctionId(1),
            RoadSegmentId1 = new RoadSegmentId(1),
            RoadSegmentId2 = new RoadSegmentId(2),
            Provenance = Provenance
        });

        await scenario.GivenAsync(new GradeJunctionWasRemoved
        {
            GradeJunctionId = new GradeJunctionId(1),
            Provenance = Provenance
        });

        Assert.True((await scenario.Load<GradeJunctionReadItem>(1))!.IsRemoved);
        Assert.DoesNotContain(new GradeJunctionId(1), (await scenario.Load<RoadSegmentReadItem>(1))!.GradeJunctionIds);
        Assert.DoesNotContain(new GradeJunctionId(1), (await scenario.Load<RoadSegmentReadItem>(2))!.GradeJunctionIds);
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
