namespace RoadRegistry.Projections.Tests.Projections.ReadProjections.StreetName;

using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.Read.Projections;
using RoadRegistry.StreetName.Events.V2;
using RoadRegistry.Tests.AggregateTests;
using RoadRegistry.ValueObjects;

public class StreetNameReadProjectionTests
{
    private readonly RoadNetworkTestDataV2 _testData = new();

    private ProvenanceData Provenance => new(_testData.Provenance);

    private ReadProjectionScenario Scenario() => new(new StreetNameReadProjection());

    [Fact]
    public async Task WhenStreetNameWasCreated_ThenStored()
    {
        var scenario = Scenario();
        var streetNameId = new StreetNameLocalId(1);

        await scenario.GivenAsync(new StreetNameWasCreated
        {
            StreetNameId = streetNameId,
            DutchName = "Teststraat",
            Provenance = Provenance
        });

        var streetName = await scenario.Load<StreetNameReadItem>((int)streetNameId);
        Assert.NotNull(streetName);
        Assert.Equal("Teststraat", streetName!.DutchName);
        Assert.False(streetName.IsRemoved);
    }

    [Fact]
    public async Task WhenStreetNameWasModified_ThenUpdated()
    {
        var scenario = Scenario();
        var streetNameId = new StreetNameLocalId(1);

        await scenario.GivenAsync(new StreetNameWasCreated
        {
            StreetNameId = streetNameId,
            DutchName = "Teststraat",
            Provenance = Provenance
        });

        await scenario.GivenAsync(new StreetNameWasModified
        {
            StreetNameId = streetNameId,
            DutchName = "Gewijzigde Teststraat",
            NisCode = "44021",
            Status = "inGebruik",
            Provenance = Provenance
        });

        var streetName = await scenario.Load<StreetNameReadItem>((int)streetNameId);
        Assert.Equal("Gewijzigde Teststraat", streetName!.DutchName);
        Assert.Equal("44021", streetName.NisCode);
        Assert.Equal("inGebruik", streetName.Status);
    }

    [Fact]
    public async Task WhenStreetNameWasModified_WithoutExistingRecord_ThenCreated()
    {
        var scenario = Scenario();
        var streetNameId = new StreetNameLocalId(1);

        await scenario.GivenAsync(new StreetNameWasModified
        {
            StreetNameId = streetNameId,
            DutchName = "Teststraat",
            NisCode = null,
            Status = null,
            Provenance = Provenance
        });

        var streetName = await scenario.Load<StreetNameReadItem>((int)streetNameId);
        Assert.NotNull(streetName);
        Assert.Equal("Teststraat", streetName!.DutchName);
    }

    [Fact]
    public async Task WhenStreetNameWasRemoved_ThenMarkedRemoved()
    {
        var scenario = Scenario();
        var streetNameId = new StreetNameLocalId(1);

        await scenario.GivenAsync(new StreetNameWasCreated
        {
            StreetNameId = streetNameId,
            DutchName = "Teststraat",
            Provenance = Provenance
        });

        await scenario.GivenAsync(new StreetNameWasRemoved
        {
            StreetNameId = streetNameId,
            Provenance = Provenance
        });

        var streetName = await scenario.Load<StreetNameReadItem>((int)streetNameId);
        Assert.True(streetName!.IsRemoved);
    }

    [Fact]
    public async Task WhenStreetNameWasRemoved_WithoutExistingRecord_ThenNoop()
    {
        var scenario = Scenario();

        await scenario.GivenAsync(new StreetNameWasRemoved
        {
            StreetNameId = new StreetNameLocalId(999),
            Provenance = Provenance
        });

        var streetName = await scenario.Load<StreetNameReadItem>(999);
        Assert.Null(streetName);
    }
}
