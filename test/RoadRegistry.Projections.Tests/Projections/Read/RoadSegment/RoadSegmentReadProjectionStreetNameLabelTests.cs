namespace RoadRegistry.Projections.Tests.Projections.ReadProjections.RoadSegment;

using System.Linq;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Microsoft.Extensions.Logging.Abstractions;
using RoadRegistry.Read.Projections;
using RoadRegistry.StreetName.Events.V2;
using RoadRegistry.Tests.AggregateTests;

public class RoadSegmentReadProjectionStreetNameLabelTests
{
    private readonly RoadNetworkTestDataV2 _testData = new();

    private ProvenanceData Provenance => new(_testData.Provenance);

    private async Task<ReadProjectionScenario> GivenSegmentWithStreetName100()
    {
        var scenario = new ReadProjectionScenario(
            new StreetNameReadProjection(),
            new RoadNodeReadProjection(),
            new RoadSegmentReadProjection(new FakeStreetNameClient(), NullLogger<RoadSegmentReadProjection>.Instance));

        var segment = _testData.Segment1Added with
        {
            StreetNameId = StreetNameAttributeBuilder.Single(_testData.Segment1Added.Geometry, new StreetNameLocalId(100))
        };

        await scenario.GivenAsync(
            new StreetNameWasCreated
            {
                StreetNameId = new StreetNameLocalId(100),
                DutchName = "Oude straat",
                Provenance = Provenance
            },
            _testData.Segment1StartNodeAdded, _testData.Segment1EndNodeAdded, segment);
        return scenario;
    }

    private static string? DutchNameFor(RoadSegmentReadItem segment, int streetNameId)
        => segment.StreetNameId.Values
            .Where(v => v.Value is not null && v.Value.StreetNameId == new StreetNameLocalId(streetNameId))
            .Select(v => v.Value!.DutchName)
            .FirstOrDefault();

    [Fact]
    public async Task WhenStreetNameWasModified_ThenLabelIsRefreshed()
    {
        var scenario = await GivenSegmentWithStreetName100();

        await scenario.GivenAsync(new StreetNameWasModified
        {
            StreetNameId = new StreetNameLocalId(100),
            DutchName = "Nieuwe straat",
            NisCode = "44021",
            Status = "Current",
            Provenance = Provenance
        });

        var segment = await scenario.Load<RoadSegmentReadItem>(1);
        Assert.Equal("Nieuwe straat", DutchNameFor(segment!, 100));
    }

    [Fact]
    public async Task WhenStreetNameWasRemoved_ThenLabelIsCleared()
    {
        var scenario = await GivenSegmentWithStreetName100();

        await scenario.GivenAsync(new StreetNameWasRemoved
        {
            StreetNameId = new StreetNameLocalId(100),
            Provenance = Provenance
        });

        var segment = await scenario.Load<RoadSegmentReadItem>(1);
        Assert.Null(DutchNameFor(segment!, 100));
    }

    [Fact]
    public async Task WhenStreetNameWasCreatedWithNoLinkedSegments_ThenNothingChanges()
    {
        var scenario = await GivenSegmentWithStreetName100();

        await scenario.GivenAsync(new StreetNameWasCreated
        {
            StreetNameId = new StreetNameLocalId(999),
            DutchName = "Onbekend",
            Provenance = Provenance
        });

        var segment = await scenario.Load<RoadSegmentReadItem>(1);
        Assert.Equal("Oude straat", DutchNameFor(segment!, 100));
    }
}
