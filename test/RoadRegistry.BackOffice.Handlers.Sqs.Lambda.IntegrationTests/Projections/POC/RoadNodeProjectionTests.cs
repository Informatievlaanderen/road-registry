namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.Projections.POC;

using AutoFixture;
using NetTopologySuite.Geometries;
using RoadRegistry.RoadNetwork.ValueObjects;
using RoadRegistry.RoadNode.Events;
using RoadRegistry.Tests.BackOffice.Scenarios;

//TODO-pr nog te verplaatsen, welke assembly? dit is geen lambda, enkel Marten

[Collection("DockerFixtureCollection")]
public class RoadNodeProjectionTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _databaseFixture;

    public RoadNodeProjectionTests(DatabaseFixture databaseFixture)
    {
        _databaseFixture = databaseFixture;
    }

    [Fact]
    public async Task WhenRoadNodeAdded_ThenSucceeded()
    {
        var fixture = new RoadNetworkTestData().ObjectProvider;

        var roadNode1Added = new RoadNodeAdded
        {
            Id = new RoadNodeId(1),
            Type = fixture.Create<RoadNodeType>(),
            Geometry = new GeometryObject(0, fixture.Create<Point>().AsText()),
            TemporaryId = fixture.Create<RoadNodeId>(),
            OriginalId = fixture.Create<RoadNodeId>()
        };

        var expectedRoadNode = new RoadNodeProjectionItem
        {
            Id = roadNode1Added.Id,
            Type = roadNode1Added.Type
        };

        await CreateProjectionTestRunner()
            .Given(roadNode1Added)
            .Expect(expectedRoadNode);
    }

    private MartenProjectionTestRunner CreateProjectionTestRunner()
    {
        return new MartenProjectionTestRunner(_databaseFixture)
            .ConfigureRoadNetworkChangesProjection<RoadNodeProjection>(RoadNodeProjection.Configure);
    }
}
