namespace RoadRegistry.Tests.BackOffice.Scenarios.WhenRemovingRoadSegments;

using Framework.Testing;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RemoveRoadSegments = RoadRegistry.BackOffice.Messages.RemoveRoadSegments;

public class GivenRoadNetwork: RoadNetworkTestBase
{
    public GivenRoadNetwork(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Fact]
    public Task WhenRemovingSegmentWithEndNode()
    {
        //TODO-pr scenario invullen

        // var pointA = new Point(new CoordinateM(0.0, 0.0, 0.0)) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        // var nodeA = ObjectProvider.Create<RoadNodeId>();
        // var pointB = new Point(new CoordinateM(10.0, 0.0, 10.0)) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        // var nodeB = ObjectProvider.Create<RoadNodeId>();
        // var pointC = new Point(new CoordinateM(0.0, 10.0, 0.0)) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        // var nodeC = ObjectProvider.Create<RoadNodeId>();
        // var pointD = new Point(new CoordinateM(10.0, 10.0, 10.0)) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        // var nodeD = ObjectProvider.Create<RoadNodeId>();
        // var pointCModified = new Point(new CoordinateM(5.0, 10.0, 0.0)) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        // var pointDModified = new Point(new CoordinateM(5.0, -10.0, 20.0)) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        // var segment1 = ObjectProvider.Create<RoadSegmentId>();
        // var segment2 = ObjectProvider.Create<RoadSegmentId>();
        // var line1 = new MultiLineString(
        //     new[]
        //     {
        //         new LineString(
        //             new CoordinateArraySequence(new[] { pointA.Coordinate, pointB.Coordinate }),
        //             GeometryConfiguration.GeometryFactory
        //         )
        //     }) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        // var line2Before = new MultiLineString(
        //     new[]
        //     {
        //         new LineString(
        //             new CoordinateArraySequence(new[] { pointC.Coordinate, pointD.Coordinate }),
        //             GeometryConfiguration.GeometryFactory
        //         )
        //     }) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        // var line2After = new MultiLineString(
        //     new[]
        //     {
        //         new LineString(
        //             new CoordinateArraySequence(new[] { pointCModified.Coordinate, pointDModified.Coordinate }),
        //             GeometryConfiguration.GeometryFactory
        //         )
        //     }) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        //
        // var count = 3;

        var removeRoadSegments = new RemoveRoadSegments
        {
            GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Measured,
            Ids = [1]
        };

        var initial = new RoadNetworkChangesAcceptedBuilder(TestData)
            .WithRoadNodeAdded(TestData.StartNode1Added)
            .WithRoadNodeAdded(TestData.EndNode1Added)
            .WithRoadSegmentAdded(TestData.Segment1Added)
            .Build();

        var command = new ChangeRoadNetworkBuilder(TestData)
            .WithRemoveRoadSegments(removeRoadSegments.Ids)
            .Build();

        var expected = new RoadNetworkChangesAcceptedBuilder(TestData)
            .WithClock(Clock)
            .WithTransactionId(2)
            .Build();

        return Run(scenario =>
            scenario
                .Given(Organizations.ToStreamName(TestData.ChangedByOrganization), TestData.ChangedByImportedOrganization)
                .Given(RoadNetworks.Stream, initial)
                .When(command)
                .Then(RoadNetworks.Stream, expected)
        );
    }
}
