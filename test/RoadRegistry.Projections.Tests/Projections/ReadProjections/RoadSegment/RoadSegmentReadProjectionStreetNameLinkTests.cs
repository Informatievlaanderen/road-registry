namespace RoadRegistry.Projections.Tests.Projections.ReadProjections;

using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Microsoft.Extensions.Logging.Abstractions;
using RoadRegistry.Read.Projections;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.Tests;
using RoadRegistry.Tests.AggregateTests;

/// <summary>
/// Verifies the <see cref="StreetNameRoadSegmentsLink"/> reverse index that <see cref="RoadSegmentReadProjection"/>
/// maintains (via SyncStreetNameLinks) so street-name label changes can find the affected road segments.
/// </summary>
public class RoadSegmentReadProjectionStreetNameLinkTests
{
    private readonly RoadNetworkTestDataV2 _testData = new();

    private ProvenanceData Provenance => new(_testData.Provenance);

    private ReadProjectionScenario Scenario()
    {
        return new ReadProjectionScenario(
            new RoadNodeReadProjection(),
            new RoadSegmentReadProjection(new FakeStreetNameClient(), NullLogger<RoadSegmentReadProjection>.Instance));
    }

    private RoadSegmentWasAdded Segment1With(RoadSegmentDynamicAttributeValues<StreetNameLocalId> streetNameId)
        => _testData.Segment1Added with { StreetNameId = streetNameId };

    [Fact]
    public async Task WhenSegmentReferencesAStreetName_ThenLinkIsCreated()
    {
        var scenario = Scenario();
        var segment = Segment1With(StreetNameAttributeBuilder.Single(_testData.Segment1Added.Geometry, new StreetNameLocalId(100)));

        await scenario.GivenAsync(_testData.Segment1StartNodeAdded, _testData.Segment1EndNodeAdded, segment);

        var link = await scenario.Load<StreetNameRoadSegmentsLink>(100);
        Assert.NotNull(link);
        Assert.Contains(new RoadSegmentId(1), link.RoadSegmentIds);
    }

    [Fact]
    public async Task WhenSegmentHasDifferentLeftAndRightStreetNames_ThenBothAreLinked()
    {
        var scenario = Scenario();
        var segment = Segment1With(StreetNameAttributeBuilder.LeftRight(_testData.Segment1Added.Geometry, new StreetNameLocalId(100), new StreetNameLocalId(200)));

        await scenario.GivenAsync(_testData.Segment1StartNodeAdded, _testData.Segment1EndNodeAdded, segment);

        Assert.Contains(new RoadSegmentId(1), (await scenario.Load<StreetNameRoadSegmentsLink>(100))!.RoadSegmentIds);
        Assert.Contains(new RoadSegmentId(1), (await scenario.Load<StreetNameRoadSegmentsLink>(200))!.RoadSegmentIds);
    }

    [Fact]
    public async Task WhenSegmentStreetNameChanges_ThenStaleLinkIsPrunedAndNewOneCreated()
    {
        var scenario = Scenario();
        var segment = Segment1With(StreetNameAttributeBuilder.Single(_testData.Segment1Added.Geometry, new StreetNameLocalId(100)));

        await scenario.GivenAsync(_testData.Segment1StartNodeAdded, _testData.Segment1EndNodeAdded, segment);
        await scenario.GivenAsync(new RoadSegmentWasModified
        {
            RoadSegmentId = new RoadSegmentId(1),
            StreetNameId = StreetNameAttributeBuilder.Single(_testData.Segment1Added.Geometry, new StreetNameLocalId(300)),
            Provenance = Provenance
        });

        Assert.Null(await scenario.Load<StreetNameRoadSegmentsLink>(100));
        Assert.Contains(new RoadSegmentId(1), (await scenario.Load<StreetNameRoadSegmentsLink>(300))!.RoadSegmentIds);
    }

    [Fact]
    public async Task WhenSegmentHasNotApplicableStreetName_ThenNoLinkIsCreated()
    {
        var scenario = Scenario();
        var segment = Segment1With(StreetNameAttributeBuilder.Single(_testData.Segment1Added.Geometry, StreetNameLocalId.NotApplicable));

        await scenario.GivenAsync(_testData.Segment1StartNodeAdded, _testData.Segment1EndNodeAdded, segment);

        Assert.Null(await scenario.Load<StreetNameRoadSegmentsLink>(StreetNameLocalId.NotApplicable.ToInt32()));
    }

    [Fact]
    public async Task WhenMultipleSegmentsShareAStreetName_ThenLinkTracksAllAndPrunesOnRemoval()
    {
        var scenario = Scenario();
        var segment1 = Segment1With(StreetNameAttributeBuilder.Single(_testData.Segment1Added.Geometry, new StreetNameLocalId(100)));
        var segment2 = _testData.Segment2Added with { StreetNameId = StreetNameAttributeBuilder.Single(_testData.Segment2Added.Geometry, new StreetNameLocalId(100)) };

        await scenario.GivenAsync(
            _testData.Segment1StartNodeAdded, _testData.Segment1EndNodeAdded,
            _testData.Segment2StartNodeAdded, _testData.Segment2EndNodeAdded,
            segment1, segment2);

        var link = await scenario.Load<StreetNameRoadSegmentsLink>(100);
        Assert.Equal(2, link!.RoadSegmentIds.Count);
        Assert.Contains(new RoadSegmentId(1), link.RoadSegmentIds);
        Assert.Contains(new RoadSegmentId(2), link.RoadSegmentIds);

        await scenario.GivenAsync(new RoadSegmentWasRemoved { RoadSegmentId = new RoadSegmentId(1), Provenance = Provenance });
        var afterFirstRemoval = await scenario.Load<StreetNameRoadSegmentsLink>(100);
        Assert.Equal(new[] { new RoadSegmentId(2) }, afterFirstRemoval!.RoadSegmentIds);

        await scenario.GivenAsync(new RoadSegmentWasRemoved { RoadSegmentId = new RoadSegmentId(2), Provenance = Provenance });
        Assert.Null(await scenario.Load<StreetNameRoadSegmentsLink>(100));
    }
}
