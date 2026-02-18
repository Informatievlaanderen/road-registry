namespace RoadRegistry.Tests.AggregateTests.GradeSeparatedJunction.ModifyGradeSeparatedJunction;

using AutoFixture;
using Extensions;
using FluentAssertions;
using Framework;
using NetTopologySuite.Geometries;
using RoadRegistry.GradeSeparatedJunction.Changes;
using ValueObjects.Problems;

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
                .Add(Fixture.Create<AddGradeSeparatedJunctionChange>() with
                {
                    LowerRoadSegmentId = TestData.AddSegment1.TemporaryId,
                    UpperRoadSegmentId = TestData.AddSegment2.TemporaryId,
                    Type = GradeSeparatedJunctionTypeV2.Brug
                })
            )
            .When(changes => changes
                .Add(new ModifyGradeSeparatedJunctionChange
                {
                    GradeSeparatedJunctionId = new GradeSeparatedJunctionId(1),
                    Type = GradeSeparatedJunctionTypeV2.Tunnel
                })
            )
            .Then((result, events) =>
            {
                result.Summary.GradeSeparatedJunctions.Modified.Should().HaveCount(1);
            })
        );
    }

    [Fact]
    public Task WhenNotFound_ThenError()
    {
        var change = Fixture.Create<ModifyGradeSeparatedJunctionChange>();

        return Run(scenario => scenario
            .Given(given => given)
            .When(changes => changes
                .Add(change)
            )
            .ThenProblems(new Error("GradeSeparatedJunctionNotFound", new ProblemParameter("Identifier", change.GradeSeparatedJunctionId.ToString())))
        );
    }
}
