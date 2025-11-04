namespace RoadRegistry.Projections.IntegrationTests;

using System.Threading.Tasks;
using Ductus.FluentDocker.Services;
using Xunit;

[CollectionDefinition("DockerFixtureCollection")]
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
        //_docker = DockerComposer.Compose("postgres.yml", "road-integration-tests-postgres"); //TODO-pr temp disable
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        //_docker.Dispose(); //TODO-pr temp disable
        return Task.CompletedTask;
    }
}
