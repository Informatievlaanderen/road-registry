namespace RoadRegistry.Tests.AggregateTests.GradeSeparatedJunction;

using AutoFixture;
using Extensions;
using Framework;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.GradeSeparatedJunction.Changes;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.ValueObjects;
using ValueObjects.Problems;

public class VerifyTopologyTests : RoadNetworkTestBase
{
    [Fact]
    public Task WhenUpperRoadSegmentIsUnknown_ThenError()
    {
        var unknownRoadSegmentId = Fixture.Create<RoadSegmentId>();

        return Run(scenario => scenario
            .Given(given => given)
            .When(changes => changes
                .Add(Fixture.Create<AddGradeSeparatedJunctionChange>() with
                {
                    UpperRoadSegmentId = unknownRoadSegmentId
                })
            )
            .ThenContainsProblems(new Error("GradeSeparatedJunctionUpperRoadSegmentMissing",
                new ProblemParameter("RoadSegmentId", unknownRoadSegmentId.ToString())))
        );
    }

    [Fact]
    public Task WhenUpperRoadSegmentIsRemoved_ThenError()
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
                .Add(Fixture.Create<AddGradeSeparatedJunctionChange>() with
                {
                    UpperRoadSegmentId = TestData.Segment1Added.RoadSegmentId
                })
            )
            .ThenContainsProblems(new Error("GradeSeparatedJunctionUpperRoadSegmentMissing",
                new ProblemParameter("RoadSegmentId", TestData.Segment1Added.RoadSegmentId.ToString())))
        );
    }

    [Fact]
    public Task WhenLowerRoadSegmentIsUnknown_ThenError()
    {
        var unknownRoadSegmentId = Fixture.Create<RoadSegmentId>();

        return Run(scenario => scenario
            .Given(given => given)
            .When(changes => changes
                .Add(Fixture.Create<AddGradeSeparatedJunctionChange>() with
                {
                    LowerRoadSegmentId = unknownRoadSegmentId
                })
            )
            .ThenContainsProblems(new Error("GradeSeparatedJunctionLowerRoadSegmentMissing",
                new ProblemParameter("RoadSegmentId", unknownRoadSegmentId.ToString())))
        );
    }

    [Fact]
    public Task WhenLowerRoadSegmentIsRemoved_ThenError()
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
                .Add(Fixture.Create<AddGradeSeparatedJunctionChange>() with
                {
                    LowerRoadSegmentId = TestData.Segment1Added.RoadSegmentId
                })
            )
            .ThenContainsProblems(new Error("GradeSeparatedJunctionLowerRoadSegmentMissing",
                new ProblemParameter("RoadSegmentId", TestData.Segment1Added.RoadSegmentId.ToString())))
        );
    }

    [Fact]
    public Task WhenUpperAndLowerSegmentsDoNotIntersect_ThenError()
    {
        var segment1Start = new Point(0, 0);
        var segment1End = new Point(10, 0);
        var segment2Start = new Point(0, 5);
        var segment2End = new Point(10, 5);

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
            .When(changes => changes
                .Add(new AddGradeSeparatedJunctionChange
                {
                    TemporaryId = Fixture.Create<GradeSeparatedJunctionId>(),
                    LowerRoadSegmentId = TestData.Segment1Added.RoadSegmentId,
                    UpperRoadSegmentId = TestData.Segment2Added.RoadSegmentId,
                    Type = Fixture.Create<GradeSeparatedJunctionType>()
                })
            )
            .ThenContainsProblems(new Error("GradeSeparatedJunctionUpperAndLowerRoadSegmentDoNotIntersect",
                new ProblemParameter("LowerRoadSegmentId", TestData.Segment1Added.RoadSegmentId.ToString()),
                new ProblemParameter("UpperRoadSegmentId", TestData.Segment2Added.RoadSegmentId.ToString())
            ))
        );
    }

    private static MultiLineString BuildSegmentGeometry(Point start, Point end)
    {
        return new MultiLineString([new LineString([start.Coordinate, end.Coordinate])])
            .WithMeasureOrdinates();
    }
}
