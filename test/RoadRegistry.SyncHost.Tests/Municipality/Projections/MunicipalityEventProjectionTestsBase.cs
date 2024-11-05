namespace RoadRegistry.SyncHost.Tests.Municipality.Projections;

using AutoFixture;
using BackOffice;
using Be.Vlaanderen.Basisregisters.GrAr.Contracts.MunicipalityRegistry;
using RoadRegistry.Tests.BackOffice;

public abstract class MunicipalityEventProjectionTestsBase : IClassFixture<MunicipalityEventProjectionClassFixture>
{
    protected readonly MunicipalityEventProjectionClassFixture Projector;
    protected readonly Fixture Fixture;

    protected MunicipalityEventProjectionTestsBase(MunicipalityEventProjectionClassFixture projector)
    {
        Projector = projector;
        Fixture = new Fixture();
    }

    protected async Task<MunicipalityWasRegistered> GivenRegisteredMunicipality()
    {
        var @event = new MunicipalityWasRegistered(
            Fixture.Create<string>(),
            Fixture.Create<string>(),
            null);

        await Projector.ProjectAsync(@event);

        return @event;
    }
}
