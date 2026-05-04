namespace RoadRegistry.Tests.AggregateTests.RoadSegment;

using Extensions;
using FluentAssertions;
using Framework;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.Extensions;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadSegment.Changes;
using ValueObjects.Problems;

public class VerifyTopologyTests : RoadNetworkTestBase
{
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
    public Task WhenGeometryIntersectsWithOtherGeometryWithoutGradeSeparatedJunction_WithIntersectionAtStartOrEndNodes_ThenNoProblems()
    {
        var segment1 = (MultiLineString)new WKTReader().Read("MULTILINESTRING ((536627.5295948688 702836.7157694297, 536638.5271524577 702839.9221909177, 536639.3131051877 702840.2792924363, 536651.2433877937 702845.69883323, 536660.6638213166 702849.9780495809))")
            .WithSrid(WellknownSrids.Lambert08);
        var segment2 = (MultiLineString)new WKTReader().Read("MULTILINESTRING ((536627.5295948688 702836.7157694297, 536627.5095197514 702837.3307665754, 536627.7776916977 702844.090798418, 536628.3371573288 702848.4388689501, 536631.8132410124 702883.8846881427, 536641.5456949207 702874.8630351247, 536655.0459321577 702857.4573191172, 536660.6638213166 702849.9780495809))")
            .WithSrid(WellknownSrids.Lambert08);

        return Run(scenario => scenario
            .Given(given => given
                .Add(TestData.AddSegment1StartNode with
                {
                    Geometry = segment1.GetSingleLineString().StartPoint.ToRoadNodeGeometry()
                })
                .Add(TestData.AddSegment1EndNode with
                {
                    Geometry = segment1.GetSingleLineString().EndPoint.ToRoadNodeGeometry()
                })
                .Add((TestData.AddSegment1 with
                {
                    Geometry = segment1.ToRoadSegmentGeometry(),
                    RoadSegmentIdReference = TestData.AddSegment1.RoadSegmentIdReference with
                    {
                        TempIds = null
                    }
                }).WithDynamicAttributePositionsOnEntireGeometryLength())
            )
            .When(changes => changes
                .Add((TestData.AddSegment2 with
                {
                    Geometry = segment2.ToRoadSegmentGeometry(),
                    RoadSegmentIdReference = TestData.AddSegment2.RoadSegmentIdReference with
                    {
                        TempIds = [new RoadSegmentTempId(3)]
                    }
                }).WithDynamicAttributePositionsOnEntireGeometryLength())
            )
            .Then((result, _) =>
            {
                result.Problems.Should().BeEmpty();
            })
        );
    }

    [Fact]
    public Task WhenGeometryIntersectsMultipleTimesWithOtherGeometry_ThenError()
    {
        var segment1Geometry = BuildRoadSegmentGeometry([new(0, 0), new(8, 0)]);
        var segment2Geometry = BuildRoadSegmentGeometry([new Point(2, 5), new(4, -5), new(6, 5)]);

        return Run(scenario => scenario
            .Given(given => given
                .Add(TestData.AddSegment1StartNode with
                {
                    Geometry = segment1Geometry.Value.GetSingleLineString().StartPoint.ToRoadNodeGeometry()
                })
                .Add(TestData.AddSegment1EndNode with
                {
                    Geometry = segment1Geometry.Value.GetSingleLineString().EndPoint.ToRoadNodeGeometry()
                })
                .Add((TestData.AddSegment1 with
                {
                    Geometry = segment1Geometry,
                    RoadSegmentIdReference = TestData.AddSegment1.RoadSegmentIdReference with
                    {
                        TempIds = null
                    }
                }).WithDynamicAttributePositionsOnEntireGeometryLength())
            )
            .When(changes => changes
                .Add(TestData.AddSegment2StartNode with
                {
                    Geometry = segment2Geometry.Value.GetSingleLineString().StartPoint.ToRoadNodeGeometry()
                })
                .Add(TestData.AddSegment2EndNode with
                {
                    Geometry = segment2Geometry.Value.GetSingleLineString().EndPoint.ToRoadNodeGeometry()
                })
                .Add((TestData.AddSegment2 with
                {
                    Geometry = segment2Geometry,
                    RoadSegmentIdReference = new(new RoadSegmentId(2), [new RoadSegmentTempId(3)])
                }).WithDynamicAttributePositionsOnEntireGeometryLength()))
            .ThenContainsProblems(new Error("RoadSegmentDuplicateIntersections",
                new ProblemParameter("IntersectingWegsegmentId", TestData.Segment1Added.RoadSegmentId.ToString()),
                new ProblemParameter("WegsegmentId", "2"),
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
