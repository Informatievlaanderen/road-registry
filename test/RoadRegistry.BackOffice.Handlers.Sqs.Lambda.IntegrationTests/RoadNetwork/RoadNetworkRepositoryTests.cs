namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.RoadNetwork;

using Xunit.Abstractions;

[Collection(nameof(DockerFixtureCollection))]
public class RoadNetworkRepositoryTests : RoadNetworkIntegrationTest
{
    public RoadNetworkRepositoryTests(DatabaseFixture databaseFixture, ITestOutputHelper testOutputHelper)
        : base(databaseFixture, testOutputHelper)
    {
    }

    //TODO-pr extra integrationtests dat goed die loadroadnetwork test met bestaande relevante/irrelevante data
    [Fact]
    public async Task WhenGetUnderlyingIds_WithGeometry_ThenIds()
    {
        // Arrange
        var sp = await BuildServiceProvider();

        //Given()

        // Act


        // Assert
    }
}
