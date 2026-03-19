namespace RoadRegistry.Tests.AggregateTests.RoadNode;

using AutoFixture;
using RoadRegistry.Extensions;
using FluentAssertions;
using Framework;
using NetTopologySuite.Geometries;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadNode.Events.V2;
using ValueObjects.Problems;

public class VerifyTopologyTests : RoadNetworkTestBase
{
    [Fact]
    public Task WhenAdd_ThenTypeIsDetermined()
    {
        return Run(scenario => scenario
            .Given(given => given
            )
            .When(changes => changes
                .Add(TestData.AddSegment1StartNode)
                .Add(TestData.AddSegment1EndNode)
                .Add(TestData.AddSegment1)
            )
            .Then((result, events) =>
            {
                events.Should().ContainItemsAssignableTo<RoadNodeTypeWasChanged>();
            })
        );
    }

    [Fact]
    public Task WhenGeometryIsReasonablyEqualToOtherNodeGeometry_ThenError()
    {
        return Run(scenario => scenario
            .Given(g => g)
            .When(changes => changes
                .Add(TestData.AddSegment1StartNode)
                .Add(TestData.AddSegment1EndNode with
                {
                    Geometry = new Point(TestData.AddSegment1StartNode.Geometry.Value.X + 0.0001, TestData.AddSegment1StartNode.Geometry.Value.Y).ToRoadNodeGeometry()
                })
            )
            .ThenContainsProblems(new Error("RoadNodeGeometryTaken",
                new ProblemParameter("ByOtherNode", TestData.AddSegment1StartNode.TemporaryId.ToString()),
                new ProblemParameter("WegknoopId", TestData.AddSegment1EndNode.TemporaryId.ToString())
            ))
        );
    }

    [Fact]
    public Task WhenNodeIsTooCloseToUnconnectedSegment_ThenError()
    {
        return Run(scenario => scenario
            .Given(given => given
                .Add(TestData.AddSegment1StartNode)
                .Add(TestData.AddSegment1EndNode)
                .Add(TestData.AddSegment1))
            .When(changes => changes
                .Add(new AddRoadNodeChange
                {
                    TemporaryId = new RoadNodeId(3),
                    Geometry = new Point(TestData.AddSegment1StartNode.Geometry.Value.X + 0.99, TestData.AddSegment1StartNode.Geometry.Value.Y).ToRoadNodeGeometry(),
                    OriginalId = new RoadNodeId(99),
                    Grensknoop = Fixture.Create<bool>()
                })
            )
            .ThenContainsProblems(new Error("RoadNodeTooClose",
                new ProblemParameter("WegsegmentId", TestData.Segment1Added.RoadSegmentId.ToString()),
                new ProblemParameter("WegknoopId", 99.ToString())
            ))
        );
    }

    [Fact]
    public Task WhenDetectType_WithNoSegmentsConnected_ThenError()
    {
        return Run(scenario => scenario
            .Given(b => b)
            .When(changes => changes
                .Add(TestData.AddSegment1StartNode)
            )
            .ThenProblems(
                new Error("RoadNodeNotConnectedToAnySegment",
                    new ProblemParameter("WegknoopId", TestData.AddSegment1StartNode.TemporaryId.ToString())
                ))
        );
    }

    [Fact]
    public Task WhenDetectType_WithOneSegmentConnected_ThenTypeIsEindknoop()
    {
        return Run(scenario => scenario
            .Given(given => given)
            .When(changes => changes
                .Add(TestData.AddSegment1StartNode)
                .Add(TestData.AddSegment1EndNode)
                .Add(TestData.AddSegment1 with
                {
                    Status = RoadSegmentStatusV2.Gerealiseerd
                })
            )
            .Then((_, events) =>
            {
                var wasChanged = events
                    .OfType<RoadNodeTypeWasChanged>()
                    .SingleOrDefault(x => x.RoadNodeId == TestData.Segment1StartNodeAdded.RoadNodeId && x.Type == RoadNodeTypeV2.Eindknoop);
                wasChanged.Should().NotBeNull();
            })
        );
    }

    [Fact]
    public Task WhenDetectType_WithTwoDifferentSegmentsConnectedAndNodeIsNotGrensknoop_ThenTypeIsSchijnknoop()
    {
        var point1 = new Point(600000, 600000);
        var point2 = new Point(600010, 600000);
        var point3 = new Point(600020, 600000);

        return Run(scenario => scenario
            .Given(b => b)
            .When(changes => changes
                .Add(TestData.AddSegment1StartNode with
                {
                    Geometry = RoadNodeGeometry.Create(point1),
                    Grensknoop = false
                })
                .Add(TestData.AddSegment1EndNode with
                {
                    Geometry = RoadNodeGeometry.Create(point2),
                    Grensknoop = false
                })
                .Add((TestData.AddSegment1 with
                {
                    Geometry = BuildRoadSegmentGeometry(point1, point2),
                    Status = RoadSegmentStatusV2.Gerealiseerd,
                    GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Ingemeten
                }).WithDynamicAttributePositionsOnEntireGeometryLength())
                .Add(TestData.AddSegment2EndNode with
                {
                    Geometry = RoadNodeGeometry.Create(point3),
                    Grensknoop = false
                })
                .Add((TestData.AddSegment2 with
                {
                    Geometry = BuildRoadSegmentGeometry(point2, point3),
                    Status = RoadSegmentStatusV2.Gerealiseerd,
                    GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Ingeschetst
                }).WithDynamicAttributePositionsOnEntireGeometryLength())
            )
            .Then((_, events) =>
            {
                var wasChanged = events
                    .OfType<RoadNodeTypeWasChanged>()
                    .SingleOrDefault(x => x.RoadNodeId == TestData.Segment1EndNodeAdded.RoadNodeId && x.Type == RoadNodeTypeV2.Schijnknoop);
                wasChanged.Should().NotBeNull();
            })
        );
    }

    [Fact]
    public Task WhenDetectType_WithTwoIdenticalSegmentsConnectedAndNodeIsGrensknoop_ThenTypeIsSchijnknoop()
    {
        var point1 = new Point(600000, 600000);
        var point2 = new Point(600010, 600000);
        var point3 = new Point(600020, 600000);

        return Run(scenario => scenario
            .Given(b => b)
            .When(changes => changes
                .Add(TestData.AddSegment1StartNode with
                {
                    Geometry = RoadNodeGeometry.Create(point1),
                    Grensknoop = false
                })
                .Add(TestData.AddSegment1EndNode with
                {
                    Geometry = RoadNodeGeometry.Create(point2),
                    Grensknoop = true
                })
                .Add((TestData.AddSegment1 with
                {
                    Geometry = BuildRoadSegmentGeometry(point1, point2),
                    Status = RoadSegmentStatusV2.Gerealiseerd,
                }).WithDynamicAttributePositionsOnEntireGeometryLength())
                .Add(TestData.AddSegment2EndNode with
                {
                    Geometry = RoadNodeGeometry.Create(point3),
                    Grensknoop = false
                })
                .Add((TestData.AddSegment1 with
                {
                    RoadSegmentIdReference = TestData.AddSegment2.RoadSegmentIdReference,
                    Geometry = BuildRoadSegmentGeometry(point2, point3),
                }).WithDynamicAttributePositionsOnEntireGeometryLength())
            )
            .Then((_, events) =>
            {
                var wasChanged = events
                    .OfType<RoadNodeTypeWasChanged>()
                    .SingleOrDefault(x => x.RoadNodeId == TestData.Segment1EndNodeAdded.RoadNodeId && x.Type == RoadNodeTypeV2.Schijnknoop);
                wasChanged.Should().NotBeNull();
            })
        );
    }

    [Fact]
    public Task WhenDetectType_WithTwoIdenticalSegmentsConnectedAndNodeIsNotGrensknoop_ThenError()
    {
        var point1 = new Point(600000, 600000);
        var point2 = new Point(600010, 600000);
        var point3 = new Point(600020, 600000);

        return Run(scenario => scenario
            .Given(b => b)
            .When(changes => changes
                .Add(TestData.AddSegment1StartNode with
                {
                    Geometry = RoadNodeGeometry.Create(point1),
                    Grensknoop = false
                })
                .Add(TestData.AddSegment1EndNode with
                {
                    Geometry = RoadNodeGeometry.Create(point2),
                    Grensknoop = false
                })
                .Add((TestData.AddSegment1 with
                {
                    RoadSegmentIdReference = new(TestData.AddSegment1.RoadSegmentIdReference.RoadSegmentId),
                    Geometry = BuildRoadSegmentGeometry(point1, point2),
                    Status = RoadSegmentStatusV2.Gerealiseerd,
                }).WithDynamicAttributePositionsOnEntireGeometryLength())
                .Add(TestData.AddSegment2EndNode with
                {
                    Geometry = RoadNodeGeometry.Create(point3),
                    Grensknoop = false
                })
                .Add((TestData.AddSegment1 with
                {
                    RoadSegmentIdReference = new(TestData.AddSegment2.RoadSegmentIdReference.RoadSegmentId),
                    Geometry = BuildRoadSegmentGeometry(point2, point3),
                }).WithDynamicAttributePositionsOnEntireGeometryLength())
            )
            .ThenContainsProblems(new Error("RoadNodeIsNotAllowed",
                new ProblemParameter("Wegsegment1Id", TestData.AddSegment1.RoadSegmentIdReference.RoadSegmentId.ToString()),
                new ProblemParameter("Wegsegment2Id", TestData.AddSegment2.RoadSegmentIdReference.RoadSegmentId.ToString()),
                new ProblemParameter("WegknoopId", TestData.AddSegment1EndNode.TemporaryId.ToString())
            ))
        );
    }

     [Fact]
     public Task WhenDetectType_WithThreeOrMoreSegmentsConnected_ThenTypeIsEchteKnoop()
     {
         var point1 = new Point(600000, 600000);
         var point2 = new Point(600010, 600000);
         var point3 = new Point(600020, 600000);
         var point4 = new Point(600010, 600010);

         return Run(scenario => scenario
             .Given(b => b)
             .When(changes => changes
                 .Add(TestData.AddSegment1StartNode with
                 {
                     Geometry = RoadNodeGeometry.Create(point1)
                 })
                 .Add(TestData.AddSegment1EndNode with
                 {
                     Geometry = RoadNodeGeometry.Create(point2)
                 })
                 .Add((TestData.AddSegment1 with
                 {
                     Geometry = BuildRoadSegmentGeometry(point1, point2),
                     Status = RoadSegmentStatusV2.Gerealiseerd
                 }).WithDynamicAttributePositionsOnEntireGeometryLength())
                 .Add(TestData.AddSegment2EndNode with
                 {
                     Geometry = RoadNodeGeometry.Create(point3)
                 })
                 .Add((TestData.AddSegment2 with
                 {
                     Geometry = BuildRoadSegmentGeometry(point2, point3),
                     Status = RoadSegmentStatusV2.Gerealiseerd
                 }).WithDynamicAttributePositionsOnEntireGeometryLength())
                 .Add(TestData.AddSegment3EndNode with
                 {
                     Geometry = RoadNodeGeometry.Create(point4)
                 })
                 .Add((TestData.AddSegment3 with
                 {
                     Geometry = BuildRoadSegmentGeometry(point2, point4),
                     Status = RoadSegmentStatusV2.Gerealiseerd
                 }).WithDynamicAttributePositionsOnEntireGeometryLength())
             )
             .Then((_, events) =>
             {
                 var wasChanged = events
                     .OfType<RoadNodeTypeWasChanged>()
                     .SingleOrDefault(x => x.RoadNodeId == TestData.Segment1EndNodeAdded.RoadNodeId && x.Type == RoadNodeTypeV2.EchteKnoop);
                 wasChanged.Should().NotBeNull();
             })
         );
    }
}
