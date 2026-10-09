namespace RoadRegistry.Projections.Tests.Projections.ReadProjections.RoadSegment;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Microsoft.Extensions.Logging.Abstractions;
using RoadRegistry.Read.Projections;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.Tests.AggregateTests;

/// <summary>
/// Verifies finding the road segments that reference a street name. The segments carry the street name ids
/// themselves - duplicated into the street_name_ids column by <see cref="RoadSegmentReadProjection"/> - and
/// <see cref="ReadModelQueries.FindRoadSegmentsForStreetName"/> queries them, so no document records the reverse.
/// </summary>
public class RoadSegmentReadProjectionStreetNameLookupTests
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

    private static async Task<RoadSegmentId[]> RoadSegmentIdsOf(ReadProjectionScenario scenario, int streetNameId)
    {
        var segments = await scenario.Store.FindRoadSegmentsForStreetName(new StreetNameLocalId(streetNameId), CancellationToken.None);
        return segments.Select(x => x.RoadSegmentId).ToArray();
    }

    [Fact]
    public async Task WhenSegmentReferencesAStreetName_ThenFoundForThatStreetName()
    {
        var scenario = Scenario();
        var segment = Segment1With(StreetNameAttributeBuilder.Single(_testData.Segment1Added.Geometry, new StreetNameLocalId(100)));

        await scenario.GivenAsync(_testData.Segment1StartNodeAdded, _testData.Segment1EndNodeAdded, segment);

        Assert.Equal([new RoadSegmentId(1)], await RoadSegmentIdsOf(scenario, 100));

        var stored = await scenario.Load<RoadSegmentReadItem>(1);
        Assert.Equal([100], stored!.StreetNameIds);
    }

    [Fact]
    public async Task WhenSegmentHasDifferentLeftAndRightStreetNames_ThenFoundForBoth()
    {
        var scenario = Scenario();
        var segment = Segment1With(StreetNameAttributeBuilder.LeftRight(_testData.Segment1Added.Geometry, new StreetNameLocalId(100), new StreetNameLocalId(200)));

        await scenario.GivenAsync(_testData.Segment1StartNodeAdded, _testData.Segment1EndNodeAdded, segment);

        Assert.Contains(new RoadSegmentId(1), await RoadSegmentIdsOf(scenario, 100));
        Assert.Contains(new RoadSegmentId(1), await RoadSegmentIdsOf(scenario, 200));
    }

    [Fact]
    public async Task WhenSegmentStreetNameChanges_ThenOnlyFoundForTheNewStreetName()
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

        Assert.Empty(await RoadSegmentIdsOf(scenario, 100));
        Assert.Equal([new RoadSegmentId(1)], await RoadSegmentIdsOf(scenario, 300));
    }

    [Fact]
    public async Task WhenSegmentHasNotApplicableStreetName_ThenNotFoundForIt()
    {
        var scenario = Scenario();
        var segment = Segment1With(StreetNameAttributeBuilder.Single(_testData.Segment1Added.Geometry, StreetNameLocalId.NotApplicable));

        await scenario.GivenAsync(_testData.Segment1StartNodeAdded, _testData.Segment1EndNodeAdded, segment);

        Assert.Empty(await RoadSegmentIdsOf(scenario, StreetNameLocalId.NotApplicable.ToInt32()));
        Assert.Empty((await scenario.Load<RoadSegmentReadItem>(1))!.StreetNameIds);
    }

    [Fact]
    public async Task WhenMultipleSegmentsShareAStreetName_ThenAllAreFoundAndRemovedOnesDropOut()
    {
        var scenario = Scenario();
        var segment1 = Segment1With(StreetNameAttributeBuilder.Single(_testData.Segment1Added.Geometry, new StreetNameLocalId(100)));
        var segment2 = _testData.Segment2Added with { StreetNameId = StreetNameAttributeBuilder.Single(_testData.Segment2Added.Geometry, new StreetNameLocalId(100)) };

        await scenario.GivenAsync(
            _testData.Segment1StartNodeAdded, _testData.Segment1EndNodeAdded,
            _testData.Segment2StartNodeAdded, _testData.Segment2EndNodeAdded,
            segment1, segment2);

        Assert.Equal([new RoadSegmentId(1), new RoadSegmentId(2)], await RoadSegmentIdsOf(scenario, 100));

        await scenario.GivenAsync(new RoadSegmentWasRemoved { RoadSegmentId = new RoadSegmentId(1), Provenance = Provenance });
        Assert.Equal([new RoadSegmentId(2)], await RoadSegmentIdsOf(scenario, 100));

        await scenario.GivenAsync(new RoadSegmentWasRemoved { RoadSegmentId = new RoadSegmentId(2), Provenance = Provenance });
        Assert.Empty(await RoadSegmentIdsOf(scenario, 100));
    }
}
