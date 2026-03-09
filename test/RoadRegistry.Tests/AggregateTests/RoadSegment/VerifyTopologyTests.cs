namespace RoadRegistry.Tests.AggregateTests.RoadSegment;

using Extensions;
using Framework;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadSegment.Changes;
using ValueObjects.Problems;

public class VerifyTopologyTests : RoadNetworkTestBase
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
                    ])]).WithMeasureOrdinates().ToRoadSegmentGeometry(),
                    RoadSegmentIdReference = TestData.AddSegment2.RoadSegmentIdReference with
                    {
                        TempIds = [new RoadSegmentTempId(3)]
                    }
                })
            )
            .ThenContainsProblems(
                new Error("RoadSegmentGeometryTaken",
                    new ProblemParameter("ByOtherWegsegmentId", TestData.Segment1Added.RoadSegmentId.ToString()),
                    new ProblemParameter("WegsegmentId", TestData.AddSegment2.RoadSegmentIdReference.RoadSegmentId.ToString()),
                    new ProblemParameter("WegsegmentTempIds", "3")
                ))
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
                new Error("RoadSegmentStartNodeMissing",
                    new ProblemParameter("WegsegmentId", TestData.AddSegment1.RoadSegmentIdReference.RoadSegmentId.ToString()),
                    new ProblemParameter("WegsegmentTempIds", TestData.AddSegment1.RoadSegmentIdReference.GetTempIdsAsString())
                )
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
                    RoadNodeId = new RoadNodeId(1)
                })
                .Add(TestData.AddSegment1)
            )
            .ThenContainsProblems(
                new Error("RoadSegmentStartNodeMissing",
                    new ProblemParameter("WegsegmentId", TestData.AddSegment1.RoadSegmentIdReference.RoadSegmentId.ToString()),
                    new ProblemParameter("WegsegmentTempIds", TestData.AddSegment1.RoadSegmentIdReference.GetTempIdsAsString())
                )
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
                    Geometry = new Point(-10, 0).ToRoadNodeGeometry()
                })
                .Add(TestData.AddSegment1)
            )
            .ThenContainsProblems(
                new Error("RoadSegmentStartNodeMissing",
                    new ProblemParameter("WegsegmentId", TestData.AddSegment1.RoadSegmentIdReference.RoadSegmentId.ToString()),
                    new ProblemParameter("WegsegmentTempIds", TestData.AddSegment1.RoadSegmentIdReference.GetTempIdsAsString())
                )
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
                new Error("RoadSegmentEndNodeMissing",
                    new ProblemParameter("WegsegmentId", TestData.AddSegment1.RoadSegmentIdReference.RoadSegmentId.ToString()),
                    new ProblemParameter("WegsegmentTempIds", TestData.AddSegment1.RoadSegmentIdReference.GetTempIdsAsString())
                )
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
                    RoadNodeId = new RoadNodeId(2)
                })
                .Add(TestData.AddSegment1)
            )
            .ThenContainsProblems(
                new Error("RoadSegmentEndNodeMissing",
                    new ProblemParameter("WegsegmentId", TestData.AddSegment1.RoadSegmentIdReference.RoadSegmentId.ToString()),
                    new ProblemParameter("WegsegmentTempIds", TestData.AddSegment1.RoadSegmentIdReference.GetTempIdsAsString())
                )
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
                    Geometry = new Point(-10, 0).ToRoadNodeGeometry()
                })
                .Add(TestData.AddSegment1)
            )
            .ThenContainsProblems(
                new Error("RoadSegmentEndNodeMissing",
                    new ProblemParameter("WegsegmentId", TestData.AddSegment1.RoadSegmentIdReference.RoadSegmentId.ToString()),
                    new ProblemParameter("WegsegmentTempIds", TestData.AddSegment1.RoadSegmentIdReference.GetTempIdsAsString())
                )
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
                    Geometry = segment1Start.ToRoadNodeGeometry()
                })
                .Add(TestData.AddSegment1EndNode with
                {
                    Geometry = segment1End.ToRoadNodeGeometry()
                })
                .Add((TestData.AddSegment1 with
                {
                    Geometry = BuildRoadSegmentGeometry(segment1Start, segment1End),
                    RoadSegmentIdReference = TestData.AddSegment1.RoadSegmentIdReference with
                    {
                        TempIds = null
                    }
                }).WithDynamicAttributePositionsOnEntireGeometryLength())
            )
            .When(changes => changes
                .Add(TestData.AddSegment2StartNode with
                {
                    Geometry = segment2Start.ToRoadNodeGeometry()
                })
                .Add(TestData.AddSegment2EndNode with
                {
                    Geometry = segment2End.ToRoadNodeGeometry()
                })
                .Add((TestData.AddSegment2 with
                {
                    Geometry = BuildRoadSegmentGeometry(segment2Start, segment2End),
                    RoadSegmentIdReference = TestData.AddSegment2.RoadSegmentIdReference with
                    {
                        TempIds = [new RoadSegmentTempId(3)]
                    }
                }).WithDynamicAttributePositionsOnEntireGeometryLength())
            )
            .ThenContainsProblems(new Error("IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction",
                new ProblemParameter("IntersectingWegsegmentId", TestData.Segment1Added.RoadSegmentId.ToString()),
                new ProblemParameter("WegsegmentId", TestData.AddSegment2.RoadSegmentIdReference.RoadSegmentId.ToString()),
                new ProblemParameter("WegsegmentTempIds", "3")
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
                new Error("RoadNodeNotConnectedToAnySegment",
                    new ProblemParameter("WegknoopId", TestData.Segment1StartNodeAdded.RoadNodeId.ToString())
                ),
                new Error("RoadNodeNotConnectedToAnySegment",
                    new ProblemParameter("WegknoopId", TestData.Segment1EndNodeAdded.RoadNodeId.ToString())
                )
            )
        );
    }
}
