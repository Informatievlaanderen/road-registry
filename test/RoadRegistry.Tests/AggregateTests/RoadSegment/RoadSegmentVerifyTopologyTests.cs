namespace RoadRegistry.Tests.AggregateTests.RoadSegment;

using Framework;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadSegment.Changes;

public class RoadSegmentVerifyTopologyTests : RoadNetworkTestBase
{
    [Fact]
    public Task WhenGeometryIsReasonablyEqualToOtherSegmentGeometry_ThenError()
    {
        return Run(scenario => scenario
            .Given(given => given
                .Add(TestData.AddSegment1StartNode)
                .Add(TestData.AddSegment1EndNode)
                .Add(TestData.AddSegment1)
            )
            .When(changes => changes
                .Add(TestData.AddSegment2 with
                {
                    Geometry = new MultiLineString([new LineString([
                        TestData.StartPoint1.Coordinate,
                        TestData.MiddlePoint1.Coordinate,
                        new Coordinate(TestData.EndPoint1.Coordinate.X + 0.0001, TestData.EndPoint1.Coordinate.Y)
                    ])]).WithMeasureOrdinates()
                })
            )
            .ThenContainsProblems(new Error("RoadSegmentGeometryTaken", new ProblemParameter("ByOtherSegment", TestData.Segment1Added.RoadSegmentId.ToString())))
        );
    }

    [Fact]
    public Task WhenStartNodeIsUnknown_ThenError()
    {
        return Run(scenario => scenario
            .Given(given => given)
            .When(changes => changes
                .Add(TestData.AddSegment1)
            )
            .ThenContainsProblems(
                new Error("RoadSegmentStartNodeMissing", new ProblemParameter("Identifier", TestData.AddSegment1.TemporaryId.ToString()))
            )
        );
    }

    [Fact]
    public Task WhenStartNodeIsRemoved_ThenError()
    {
        return Run(scenario => scenario
            .Given(given => given)
            .When(changes => changes
                .Add(TestData.AddSegment1StartNode)
                .Add(new RemoveRoadNodeChange
                {
                    Id = new RoadNodeId(1)
                })
                .Add(TestData.AddSegment1)
            )
            .ThenContainsProblems(
                new Error("RoadSegmentStartNodeMissing", new ProblemParameter("Identifier", TestData.AddSegment1.TemporaryId.ToString()))
            )
        );
    }

    [Fact]
    public Task WhenStartNodeIsNotEqualToSegmentGeometryStartPoint_ThenError()
    {
        return Run(scenario => scenario
            .Given(given => given)
            .When(changes => changes
                .Add(TestData.AddSegment1StartNode with
                {
                    Geometry = new Point(-10, 0)
                })
                .Add(TestData.AddSegment1)
            )
            .ThenContainsProblems(
                new Error("RoadSegmentStartPointDoesNotMatchNodeGeometry", new ProblemParameter("Identifier", TestData.AddSegment1.TemporaryId.ToString()))
            )
        );
    }

    [Fact]
    public Task WhenEndNodeIsUnknown_ThenError()
    {
        return Run(scenario => scenario
            .Given(given => given)
            .When(changes => changes
                .Add(TestData.AddSegment1)
            )
            .ThenContainsProblems(
                new Error("RoadSegmentEndNodeMissing", new ProblemParameter("Identifier", TestData.AddSegment1.TemporaryId.ToString()))
            )
        );
    }

    [Fact]
    public Task WhenEndNodeIsRemoved_ThenError()
    {
        return Run(scenario => scenario
            .Given(given => given)
            .When(changes => changes
                .Add(TestData.AddSegment1StartNode)
                .Add(TestData.AddSegment1EndNode)
                .Add(new RemoveRoadNodeChange
                {
                    Id = new RoadNodeId(2)
                })
                .Add(TestData.AddSegment1)
            )
            .ThenContainsProblems(
                new Error("RoadSegmentEndNodeMissing", new ProblemParameter("Identifier", TestData.AddSegment1.TemporaryId.ToString()))
            )
        );
    }

    [Fact]
    public Task WhenEndNodeIsNotEqualToSegmentGeometryEndPoint_ThenError()
    {
        return Run(scenario => scenario
            .Given(given => given)
            .When(changes => changes
                .Add(TestData.AddSegment1StartNode)
                .Add(TestData.AddSegment1EndNode with
                {
                    Geometry = new Point(-10, 0)
                })
                .Add(TestData.AddSegment1)
            )
            .ThenContainsProblems(
                new Error("RoadSegmentEndPointDoesNotMatchNodeGeometry", new ProblemParameter("Identifier", TestData.AddSegment1.TemporaryId.ToString()))
            )
        );
    }

    [Fact]
    public Task WhenGeometryIntersectsWithOtherGeometryWithoutGradeSeparatedJunction_ThenError()
    {
        var segment1Start = new Point(0, 0);
        var segment1End = new Point(10, 0);
        var segment2Start = new Point(5, 5);
        var segment2End = new Point(10, -5);

        return Run(scenario => scenario
            .Given(given => given
                .Add(TestData.AddSegment1StartNode with
                {
                    Geometry = segment1Start
                })
                .Add(TestData.AddSegment1EndNode with
                {
                    Geometry = segment1End
                })
                .Add(TestData.AddSegment1 with
                {
                    Geometry = BuildSegmentGeometry(segment1Start, segment1End)
                })
            )
            .When(changes => changes
                .Add(TestData.AddSegment2StartNode with
                {
                    Geometry = segment2Start
                })
                .Add(TestData.AddSegment2EndNode with
                {
                    Geometry = segment2End
                })
                .Add(TestData.AddSegment2 with
                {
                    Geometry = BuildSegmentGeometry(segment2Start, segment2End)
                })
            )
            .ThenContainsProblems(new Error("IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction",
                new ProblemParameter("ModifiedRoadSegmentId", TestData.AddSegment2.TemporaryId.ToString()),
                new ProblemParameter("IntersectingRoadSegmentId", TestData.Segment1Added.RoadSegmentId.ToString())
            ))
        );
    }

    [Fact]
    public Task WhenOnlySegmentIsRemoved_ThenRoadNodeVerifyTopologyIsUsedToReturnError()
    {
        return Run(scenario => scenario
            .Given(given => given
                .Add(TestData.AddSegment1StartNode)
                .Add(TestData.AddSegment1EndNode)
                .Add(TestData.AddSegment1))
            .When(changes => changes
                .Add(new RemoveRoadSegmentChange
                {
                    RoadSegmentId = TestData.Segment1Added.RoadSegmentId
                })
            )
            .ThenProblems(
                new Error("RoadNodeNotConnectedToAnySegment", new ProblemParameter("RoadNodeId", TestData.Segment1StartNodeAdded.RoadNodeId.ToString())),
                new Error("RoadNodeNotConnectedToAnySegment", new ProblemParameter("RoadNodeId", TestData.Segment1EndNodeAdded.RoadNodeId.ToString()))
            )
        );
    }

    private static MultiLineString BuildSegmentGeometry(Point start, Point end)
    {
        return new MultiLineString([new LineString([start.Coordinate, end.Coordinate])])
            .WithMeasureOrdinates();
    }
}
