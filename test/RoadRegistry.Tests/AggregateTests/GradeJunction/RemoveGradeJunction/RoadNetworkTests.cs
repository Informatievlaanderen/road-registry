namespace RoadRegistry.Tests.AggregateTests.GradeJunction.RemoveGradeJunction;

using AutoFixture;
using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.GradeJunction.Changes;
using RoadRegistry.Tests.AggregateTests.Framework;

public class ScopedRoadNetworkTests : RoadNetworkTestBase
{
    [Fact]
    public Task ThenSummaryIsUpdated()
    {
        var segment1Start = new Point(0, 0);
        var segment1End = new Point(10, 0);
        var segment2Start = new Point(5, 5);
        var segment2End = new Point(5, -5);

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
                    Geometry = BuildRoadSegmentGeometry(segment1Start, segment1End)
                }).WithDynamicAttributePositionsOnEntireGeometryLength())
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
                    Geometry = BuildRoadSegmentGeometry(segment2Start, segment2End)
                }).WithDynamicAttributePositionsOnEntireGeometryLength())
                .Add(Fixture.Create<AddGradeJunctionChange>() with
                {
                    LowerRoadSegmentId = TestData.AddSegment1.RoadSegmentIdReference.RoadSegmentId,
                    UpperRoadSegmentId = TestData.AddSegment2.RoadSegmentIdReference.RoadSegmentId
                })
            )
            .When(changes => changes
                .Add(new RemoveGradeJunctionChange
                {
                    GradeJunctionId = new GradeJunctionId(1)
                })
            )
            .Then((result, events) =>
            {
                result.Summary.GradeJunctions.Removed.Should().HaveCount(1);
            })
        );
    }

    [Fact]
    public Task WhenNotFound_ThenNoProblem()
    {
        var change = Fixture.Create<RemoveGradeJunctionChange>();

        return Run(scenario => scenario
            .Given(given => given)
            .When(changes => changes
                .Add(change)
            )
            .Then((result, events) =>
            {
                result.Problems.HasError().Should().BeFalse();
            })
        );
    }
}
