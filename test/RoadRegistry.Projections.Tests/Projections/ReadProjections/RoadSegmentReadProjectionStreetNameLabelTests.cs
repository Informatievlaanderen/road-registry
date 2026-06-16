namespace RoadRegistry.Projections.Tests.Projections.ReadProjections;

using System.Linq;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.Read.Projections;
using RoadRegistry.StreetName.Events.V2;
using RoadRegistry.Tests;
using RoadRegistry.Tests.AggregateTests;

public class RoadSegmentReadProjectionStreetNameLabelTests
{
    private readonly RoadNetworkTestDataV2 _testData = new();

    private ProvenanceData Provenance => new(_testData.Provenance);

    private async Task<ReadProjectionScenario> GivenSegmentWithStreetName100()
    {
        var cache = new FakeStreetNameCache().AddStreetName(100, "Oude straat", "inGebruik");
        var scenario = new ReadProjectionScenario(
            new RoadNodeReadProjection(),
            new RoadSegmentReadProjection(cache, new FakeStreetNameClient()));

        var segment = _testData.Segment1Added with
        {
            StreetNameId = StreetNameAttributeBuilder.Single(_testData.Segment1Added.Geometry, new StreetNameLocalId(100))
        };

        await scenario.GivenAsync(_testData.Segment1StartNodeAdded, _testData.Segment1EndNodeAdded, segment);
        return scenario;
    }

    private static string? DutchNameFor(RoadSegmentReadItem segment, int streetNameId)
        => segment.StreetNameId.Values
            .Where(v => v.Value is not null && v.Value.StreetNameId == new StreetNameLocalId(streetNameId))
            .Select(v => v.Value!.DutchName)
            .FirstOrDefault();

    [Fact]
    public async Task WhenStreetNameNameWasModified_ThenLabelIsRefreshed()
    {
        var scenario = await GivenSegmentWithStreetName100();

        await scenario.GivenAsync(new StreetNameNameWasModified
        {
            StreetNameId = new StreetNameLocalId(100),
            DutchName = "Nieuwe straat",
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
