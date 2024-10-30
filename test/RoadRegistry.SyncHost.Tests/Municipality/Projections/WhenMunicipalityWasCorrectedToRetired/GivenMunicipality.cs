namespace RoadRegistry.SyncHost.Tests.Municipality.Projections.WhenMunicipalityWasCorrectedToRetired;

using AutoFixture;
using Be.Vlaanderen.Basisregisters.GrAr.Contracts.MunicipalityRegistry;
using FluentAssertions;
using Sync.MunicipalityRegistry.Models;

public class GivenMunicipality : MunicipalityEventProjectionTestsBase
{
    public GivenMunicipality(MunicipalityEventProjectionClassFixture projector)
        : base(projector)
    {
    }

    [Fact]
    public async Task ThenUpdated()
    {
        var registeredEvent = await GivenRegisteredMunicipality();

        var @event = new MunicipalityWasCorrectedToRetired(
            registeredEvent.MunicipalityId,
            Fixture.Create<string>(),
            null);

        await Projector.ProjectAsync(@event);

        var actual = await Projector.Connection.Municipalities.FindAsync([@event.MunicipalityId]);

        actual.Should().NotBeNull();
        actual!.MunicipalityId.Should().Be(@event.MunicipalityId);
        actual.NisCode.Should().Be(registeredEvent.NisCode);
        actual.Status.Should().Be(MunicipalityStatus.Retired);
        actual.Geometry.Should().BeNull();
    }
}
