namespace RoadRegistry.Projections.Tests.Projections.ReadProjections;

using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RoadRegistry.Read.Projections;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.Tests.AggregateTests;

public class RoadSegmentReadProjectionTests
{
    private readonly RoadNetworkTestDataV2 _testData = new();

    private ReadProjectionScenario Scenario() => new(
        new RoadNodeReadProjection(),
        new RoadSegmentReadProjection(new FakeStreetNameClient(), NullLogger<RoadSegmentReadProjection>.Instance));

    private ProvenanceData Provenance => new(_testData.Provenance);

    [Fact]
    public async Task WhenOutlinedRoadSegmentWasAdded_ThenStoredWithNoNodesAndIngeschetst()
    {
        var scenario = Scenario();

        var evt = _testData.Fixture.Create<OutlinedRoadSegmentWasAdded>();
        await scenario.GivenAsync(evt);

        var segment = await scenario.Load<RoadSegmentReadItem>((int)evt.RoadSegmentId);
        Assert.NotNull(segment);
        Assert.Equal(evt.RoadSegmentId, segment.RoadSegmentId);
        Assert.True(segment.IsV2);
        Assert.Equal(evt.Status.ToString(), segment.Status);
        Assert.Equal(RoadSegmentGeometryDrawMethodV2.Ingeschetst.ToString(), segment.GeometryDrawMethod);
        Assert.Null(segment.StartNodeId);
        Assert.Null(segment.EndNodeId);
        Assert.Empty(segment.EuropeanRoadNumbers);
        Assert.Empty(segment.NationalRoadNumbers);
    }

    [Fact]
    public async Task WhenRoadSegmentWasAdded_ThenStoredAndNodesReferenceTheSegment()
    {
        var scenario = Scenario();

        await scenario.GivenAsync(_testData.Segment1StartNodeAdded, _testData.Segment1EndNodeAdded, _testData.Segment1Added);

        var segment = await scenario.Load<RoadSegmentReadItem>(1);
        Assert.NotNull(segment);
        Assert.Equal(new RoadSegmentId(1), segment.RoadSegmentId);
        Assert.True(segment.IsV2);
        Assert.Equal(RoadSegmentStatusV2.Gerealiseerd.ToString(), segment.Status);

        var startNode = await scenario.Load<RoadNodeReadItem>(1);
        var endNode = await scenario.Load<RoadNodeReadItem>(2);
        Assert.Contains(new RoadSegmentId(1), startNode!.RoadSegmentIds);
        Assert.Contains(new RoadSegmentId(1), endNode!.RoadSegmentIds);
    }

    [Fact]
    public async Task WhenRoadSegmentWasRemoved_ThenMarkedRemovedAndNodesNoLongerReferenceIt()
    {
        var scenario = Scenario();

        await scenario.GivenAsync(_testData.Segment1StartNodeAdded, _testData.Segment1EndNodeAdded, _testData.Segment1Added);
        await scenario.GivenAsync(new RoadSegmentWasRemoved { RoadSegmentId = new RoadSegmentId(1), Provenance = Provenance });

        var segment = await scenario.Load<RoadSegmentReadItem>(1);
        Assert.True(segment!.IsRemoved);

        var startNode = await scenario.Load<RoadNodeReadItem>(1);
        Assert.DoesNotContain(new RoadSegmentId(1), startNode!.RoadSegmentIds);
    }

    [Fact]
    public async Task WhenRoadSegmentWasRetired_ThenStatusGehistoreerdAndNodesCleared()
    {
        var scenario = Scenario();

        await scenario.GivenAsync(_testData.Segment1StartNodeAdded, _testData.Segment1EndNodeAdded, _testData.Segment1Added);
        await scenario.GivenAsync(new RoadSegmentWasRetired { RoadSegmentId = new RoadSegmentId(1), Provenance = Provenance });

        var segment = await scenario.Load<RoadSegmentReadItem>(1);
        Assert.Equal(RoadSegmentStatusV2.Gehistoreerd.ToString(), segment!.Status);
        Assert.Null(segment.StartNodeId);
        Assert.Null(segment.EndNodeId);
    }

    [Fact]
    public async Task WhenRoadSegmentWasModified_StatusOnly_ThenOtherFieldsKept()
    {
        var scenario = Scenario();

        await scenario.GivenAsync(_testData.Segment1StartNodeAdded, _testData.Segment1EndNodeAdded, _testData.Segment1Added);
        await scenario.GivenAsync(new RoadSegmentWasModified
        {
            RoadSegmentId = new RoadSegmentId(1),
            Status = RoadSegmentStatusV2.Gehistoreerd,
            Provenance = Provenance
        });

        var segment = await scenario.Load<RoadSegmentReadItem>(1);
        Assert.Equal(RoadSegmentStatusV2.Gehistoreerd.ToString(), segment!.Status);
        Assert.Equal(new RoadNodeId(1), segment.StartNodeId);
        Assert.Equal(new RoadNodeId(2), segment.EndNodeId);
    }

    [Fact]
    public async Task WhenCatchingUp_ThenStreetNameApiClientIsNotCalled()
    {
        var client = new FakeStreetNameClient().WithStreetName(100, "Straat via API");
        var roadSegmentProjection = new RoadSegmentReadProjection(client, NullLogger<RoadSegmentReadProjection>.Instance)
        {
            IsCatchingUp = true
        };
        var scenario = new ReadProjectionScenario(
            new RoadNodeReadProjection(),
            roadSegmentProjection);

        var segment = _testData.Segment1Added with
        {
            StreetNameId = StreetNameAttributeBuilder.Single(_testData.Segment1Added.Geometry, new StreetNameLocalId(100))
        };

        await scenario.GivenAsync(_testData.Segment1StartNodeAdded, _testData.Segment1EndNodeAdded, segment);

        var stored = await scenario.Load<RoadSegmentReadItem>(1);
        var dutchName = stored!.StreetNameId.Values
            .Where(v => v.Value?.StreetNameId == new StreetNameLocalId(100))
            .Select(v => v.Value!.DutchName)
            .FirstOrDefault();
        Assert.Null(dutchName);
        Assert.False(client.WasCalled);
    }

    [Fact]
    public async Task WhenStreetNameApiClientThrows_ThenNullIsReturnedAndErrorIsLogged()
    {
        var logger = new CapturingLogger<RoadSegmentReadProjection>();
        var client = new FakeStreetNameClient().ThatThrows(new InvalidOperationException("API unavailable"));
        var scenario = new ReadProjectionScenario(
            new RoadNodeReadProjection(),
            new RoadSegmentReadProjection(client, logger));

        var segment = _testData.Segment1Added with
        {
            StreetNameId = StreetNameAttributeBuilder.Single(_testData.Segment1Added.Geometry, new StreetNameLocalId(100))
        };

        await scenario.GivenAsync(_testData.Segment1StartNodeAdded, _testData.Segment1EndNodeAdded, segment);

        var stored = await scenario.Load<RoadSegmentReadItem>(1);
        var dutchName = stored!.StreetNameId.Values
            .Where(v => v.Value?.StreetNameId == new StreetNameLocalId(100))
            .Select(v => v.Value!.DutchName)
            .FirstOrDefault();
        Assert.Null(dutchName);
        Assert.Contains(logger.Entries, e => e.Level == LogLevel.Error && e.Exception is InvalidOperationException);
    }

    [Fact]
    public async Task WhenStreetNameNotFoundLocally_ThenApiClientIsCalledAsFallback()
    {
        const string apiName = "Straat via API";
        var client = new FakeStreetNameClient().WithStreetName(100, apiName);
        var scenario = new ReadProjectionScenario(
            new RoadNodeReadProjection(),
            new RoadSegmentReadProjection(client, NullLogger<RoadSegmentReadProjection>.Instance));

        var segment = _testData.Segment1Added with
        {
            StreetNameId = StreetNameAttributeBuilder.Single(_testData.Segment1Added.Geometry, new StreetNameLocalId(100))
        };

        await scenario.GivenAsync(_testData.Segment1StartNodeAdded, _testData.Segment1EndNodeAdded, segment);

        var stored = await scenario.Load<RoadSegmentReadItem>(1);
        var dutchName = stored!.StreetNameId.Values
            .Where(v => v.Value?.StreetNameId == new StreetNameLocalId(100))
            .Select(v => v.Value!.DutchName)
            .FirstOrDefault();
        Assert.Equal(apiName, dutchName);
    }

    [Fact]
    public async Task WhenAddedToAndRemovedFromEuropeanRoad_ThenNumbersTracked()
    {
        var scenario = Scenario();
        var number = _testData.Fixture.Create<EuropeanRoadNumber>();

        await scenario.GivenAsync(_testData.Segment2StartNodeAdded, _testData.Segment2EndNodeAdded, _testData.Segment2Added);

        await scenario.GivenAsync(new RoadSegmentWasAddedToEuropeanRoad { RoadSegmentId = new RoadSegmentId(2), Number = number, Provenance = Provenance });
        var afterAdd = await scenario.Load<RoadSegmentReadItem>(2);
        Assert.Contains(number, afterAdd!.EuropeanRoadNumbers);

        await scenario.GivenAsync(new RoadSegmentWasRemovedFromEuropeanRoad { RoadSegmentId = new RoadSegmentId(2), Number = number, Provenance = Provenance });
        var afterRemove = await scenario.Load<RoadSegmentReadItem>(2);
        Assert.DoesNotContain(number, afterRemove!.EuropeanRoadNumbers);
    }
}
