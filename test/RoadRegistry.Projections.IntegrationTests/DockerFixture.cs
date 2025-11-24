namespace RoadRegistry.Projections.IntegrationTests;

using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.DockerUtilities;
using Ductus.FluentDocker.Services;
using Xunit;

[CollectionDefinition(nameof(DockerFixtureCollection))]
public class DockerFixtureCollection : ICollectionFixture<DockerFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}

public class DockerFixture : IAsyncLifetime
{
    private ICompositeService _docker;

    public Task InitializeAsync()
    {
        _docker = DockerComposer.Compose("postgres.yml", "road-projection-integration-tests-postgres");

        //TODO-pr onstabiel integration tests: soms is de docker nog niet helemaal klaar en dan wordt de Docker dispose al opgeroepen wanneer de InstallPostgis gebeurd, maar daar kom je dan weer niet in de exception catch
        Thread.Sleep(2000);

        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _docker.Dispose();
        return Task.CompletedTask;
    }
}
