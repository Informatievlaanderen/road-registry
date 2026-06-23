namespace RoadRegistry.Projections.Tests.Projections.ReadProjections;

using System.Threading.Tasks;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.GradeSeparatedJunction.Events.V2;
using RoadRegistry.Read.Projections;
using RoadRegistry.Tests;
using RoadRegistry.Tests.AggregateTests;
using V1 = RoadRegistry.GradeSeparatedJunction.Events.V1;

public class GradeSeparatedJunctionReadProjectionTests
{
    private readonly RoadNetworkTestDataV2 _testData = new();

    private ProvenanceData Provenance => new(_testData.Provenance);

    private ReadProjectionScenario Scenario() => new(
        new RoadNodeReadProjection(),
        new RoadSegmentReadProjection(),
        new GradeSeparatedJunctionReadProjection());

    // Grade separated junctions reference existing road segments, so those segments (and their nodes) must be projected first.
    private Task GivenRoadSegments1To3(ReadProjectionScenario scenario) => scenario.GivenAsync(
        _testData.Segment1StartNodeAdded, _testData.Segment1EndNodeAdded,
        _testData.Segment2StartNodeAdded, _testData.Segment2EndNodeAdded,
        _testData.Segment3StartNodeAdded, _testData.Segment3EndNodeAdded,
        _testData.Segment1Added, _testData.Segment2Added, _testData.Segment3Added);

    [Fact]
    public async Task WhenV1GradeSeparatedJunctionAdded_ThenStoredAndRoadSegmentsReferenceIt()
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

        Assert.Contains(new GradeSeparatedJunctionId(1), (await scenario.Load<RoadSegmentReadItem>(1))!.GradeSeparatedJunctionIds);
        Assert.Contains(new GradeSeparatedJunctionId(1), (await scenario.Load<RoadSegmentReadItem>(2))!.GradeSeparatedJunctionIds);
    }

    [Fact]
    public async Task WhenV1GradeSeparatedJunctionModified_ThenReassignsRoadSegmentLinks()
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
        Assert.DoesNotContain(new GradeSeparatedJunctionId(1), (await scenario.Load<RoadSegmentReadItem>(1))!.GradeSeparatedJunctionIds);
        Assert.Contains(new GradeSeparatedJunctionId(1), (await scenario.Load<RoadSegmentReadItem>(3))!.GradeSeparatedJunctionIds);
        Assert.Contains(new GradeSeparatedJunctionId(1), (await scenario.Load<RoadSegmentReadItem>(2))!.GradeSeparatedJunctionIds);
    }

    [Fact]
    public async Task WhenV1GradeSeparatedJunctionRemoved_ThenMarkedRemovedAndLinksRemoved()
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
        Assert.DoesNotContain(new GradeSeparatedJunctionId(1), (await scenario.Load<RoadSegmentReadItem>(1))!.GradeSeparatedJunctionIds);
        Assert.DoesNotContain(new GradeSeparatedJunctionId(1), (await scenario.Load<RoadSegmentReadItem>(2))!.GradeSeparatedJunctionIds);
    }

    [Fact]
    public async Task WhenGradeSeparatedJunctionWasAdded_ThenStoredAndRoadSegmentsReferenceIt()
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
            Provenance = Provenance
        });

        var junction = await scenario.Load<GradeSeparatedJunctionReadItem>(1);
        Assert.Equal(type.ToString(), junction!.Type);
        Assert.True(junction.IsV2);

        Assert.Contains(new GradeSeparatedJunctionId(1), (await scenario.Load<RoadSegmentReadItem>(1))!.GradeSeparatedJunctionIds);
        Assert.Contains(new GradeSeparatedJunctionId(1), (await scenario.Load<RoadSegmentReadItem>(2))!.GradeSeparatedJunctionIds);
    }

    [Fact]
    public async Task WhenGradeSeparatedJunctionWasModified_WithNullFields_ThenKeepsExistingValuesAndLinks()
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
        Assert.Equal(new RoadSegmentId(1), junction.LowerRoadSegmentId);
        Assert.Contains(new GradeSeparatedJunctionId(1), (await scenario.Load<RoadSegmentReadItem>(1))!.GradeSeparatedJunctionIds);
        Assert.Contains(new GradeSeparatedJunctionId(1), (await scenario.Load<RoadSegmentReadItem>(2))!.GradeSeparatedJunctionIds);
    }

    [Fact]
    public async Task WhenGradeSeparatedJunctionWasRemoved_ThenMarkedRemovedAndLinksRemoved()
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
            Provenance = Provenance
        });

        await scenario.GivenAsync(new GradeSeparatedJunctionWasRemoved
        {
            GradeSeparatedJunctionId = new GradeSeparatedJunctionId(1),
            Provenance = Provenance
        });

        Assert.True((await scenario.Load<GradeSeparatedJunctionReadItem>(1))!.IsRemoved);
        Assert.DoesNotContain(new GradeSeparatedJunctionId(1), (await scenario.Load<RoadSegmentReadItem>(1))!.GradeSeparatedJunctionIds);
        Assert.DoesNotContain(new GradeSeparatedJunctionId(1), (await scenario.Load<RoadSegmentReadItem>(2))!.GradeSeparatedJunctionIds);
    }
}
