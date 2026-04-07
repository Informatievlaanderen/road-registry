namespace RoadRegistry.Tests.AggregateTests.GradeJunction.RemoveGradeJunction;

using AutoFixture;
using FluentAssertions;
using RoadRegistry.Extensions;
using RoadRegistry.GradeJunction.Events.V2;
using RoadRegistry.GradeSeparatedJunction.Changes;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.Tests.AggregateTests.Framework;
using Point = NetTopologySuite.Geometries.Point;

public class ScopedRoadNetworkTests : RoadNetworkTestBase
{
    [Fact]
    public Task GivenGradeJunction_WithChange_WhenSegmentIsRemoved_ThenGradeJunctionIsRemovedAndSummaryIsUpdated()
    {
        var segment1Start = new Point(0, 0);
        var segment1End = new Point(10, 0);
        var segment2Start = new Point(5, 5);
        var segment2End = new Point(5, -5);

        return Run(scenario => scenario
            .Given(changes => changes
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
                    Status = RoadSegmentStatusV2.Gerealiseerd,
                    CarAccessBackward = new RoadSegmentDynamicAttributeValues<bool>(true, TestData.AddSegment1.Geometry),
                    CarAccessForward = new RoadSegmentDynamicAttributeValues<bool>(true, TestData.AddSegment1.Geometry),
                    BikeAccessBackward = new RoadSegmentDynamicAttributeValues<bool>(false, TestData.AddSegment1.Geometry),
                    BikeAccessForward = new RoadSegmentDynamicAttributeValues<bool>(false, TestData.AddSegment1.Geometry),
                    PedestrianAccess = new RoadSegmentDynamicAttributeValues<bool>(false, TestData.AddSegment1.Geometry)
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
                    Geometry = BuildRoadSegmentGeometry(segment2Start, segment2End),
                    Status = RoadSegmentStatusV2.Gerealiseerd,
                    CarAccessBackward = new RoadSegmentDynamicAttributeValues<bool>(false, TestData.AddSegment2.Geometry),
                    CarAccessForward = new RoadSegmentDynamicAttributeValues<bool>(false, TestData.AddSegment2.Geometry),
                    BikeAccessBackward = new RoadSegmentDynamicAttributeValues<bool>(true, TestData.AddSegment2.Geometry),
                    BikeAccessForward = new RoadSegmentDynamicAttributeValues<bool>(true, TestData.AddSegment2.Geometry),
                    PedestrianAccess = new RoadSegmentDynamicAttributeValues<bool>(false, TestData.AddSegment2.Geometry)
                }).WithDynamicAttributePositionsOnEntireGeometryLength())
            )
            .When(changes => changes
                .Add(new RemoveRoadSegmentChange
                {
                    RoadSegmentId = TestData.Segment2Added.RoadSegmentId
                })
                .Add(new RemoveRoadNodeChange
                {
                    RoadNodeId = TestData.Segment2StartNodeAdded.RoadNodeId
                })
                .Add(new RemoveRoadNodeChange
                {
                    RoadNodeId = TestData.Segment2EndNodeAdded.RoadNodeId
                })
            )
            .Then((result, events) =>
            {
                result.Summary.GradeJunctions.Removed.Should().HaveCount(1);
                events.OfType<GradeJunctionWasRemoved>().Should().HaveCount(1);
            })
        );
    }

    [Theory]
    [InlineData(nameof(RoadSegmentStatusV2.Gepland))]
    [InlineData(nameof(RoadSegmentStatusV2.NietGerealiseerd))]
    [InlineData(nameof(RoadSegmentStatusV2.BuitenGebruik))]
    [InlineData(nameof(RoadSegmentStatusV2.Gehistoreerd))]
    public Task GivenGradeJunction_WithChange_WhenSegmentIsNoLongerGerealiseerd_ThenGradeJunctionIsRemovedAndSummaryIsUpdated(string status)
    {
        var segment1Start = new Point(0, 0);
        var segment1End = new Point(10, 0);
        var segment2Start = new Point(5, 5);
        var segment2End = new Point(5, -5);

        return Run(scenario => scenario
            .Given(changes => changes
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
                    Status = RoadSegmentStatusV2.Gerealiseerd,
                    CarAccessBackward = new RoadSegmentDynamicAttributeValues<bool>(true, TestData.AddSegment1.Geometry),
                    CarAccessForward = new RoadSegmentDynamicAttributeValues<bool>(true, TestData.AddSegment1.Geometry),
                    BikeAccessBackward = new RoadSegmentDynamicAttributeValues<bool>(false, TestData.AddSegment1.Geometry),
                    BikeAccessForward = new RoadSegmentDynamicAttributeValues<bool>(false, TestData.AddSegment1.Geometry),
                    PedestrianAccess = new RoadSegmentDynamicAttributeValues<bool>(false, TestData.AddSegment1.Geometry)
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
                    Geometry = BuildRoadSegmentGeometry(segment2Start, segment2End),
                    Status = RoadSegmentStatusV2.Gerealiseerd,
                    CarAccessBackward = new RoadSegmentDynamicAttributeValues<bool>(false, TestData.AddSegment2.Geometry),
                    CarAccessForward = new RoadSegmentDynamicAttributeValues<bool>(false, TestData.AddSegment2.Geometry),
                    BikeAccessBackward = new RoadSegmentDynamicAttributeValues<bool>(true, TestData.AddSegment2.Geometry),
                    BikeAccessForward = new RoadSegmentDynamicAttributeValues<bool>(true, TestData.AddSegment2.Geometry),
                    PedestrianAccess = new RoadSegmentDynamicAttributeValues<bool>(false, TestData.AddSegment2.Geometry)
                }).WithDynamicAttributePositionsOnEntireGeometryLength())
            )
            .When(changes => changes
                .Add(new ModifyRoadSegmentChange
                {
                    RoadSegmentIdReference = new RoadSegmentIdReference(TestData.Segment2Added.RoadSegmentId),
                    Status = RoadSegmentStatusV2.Parse(status)
                })
            )
            .Then((result, events) =>
            {
                result.Summary.GradeJunctions.Removed.Should().HaveCount(1);
                events.OfType<GradeJunctionWasRemoved>().Should().HaveCount(1);
            })
        );
    }

    [Fact]
    public Task GivenGradeJunction_WhenSegmentNoLongerIntersectsWithOtherSegment_ThenGradeJunctionIsRemovedAndSummaryIsUpdated()
    {
        var segment1Start = new Point(0, 0);
        var segment1End = new Point(10, 0);
        var segment2Start = new Point(5, 5);
        var segment2End = new Point(5, -5);

        var segment2NewLocationStart = new Point(100, 0);
        var segment2NewLocationEnd = new Point(110, 0);

        return Run(scenario => scenario
            .Given(changes => changes
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
                    Status = RoadSegmentStatusV2.Gerealiseerd,
                    CarAccessBackward = new RoadSegmentDynamicAttributeValues<bool>(true, TestData.AddSegment1.Geometry),
                    CarAccessForward = new RoadSegmentDynamicAttributeValues<bool>(true, TestData.AddSegment1.Geometry),
                    BikeAccessBackward = new RoadSegmentDynamicAttributeValues<bool>(false, TestData.AddSegment1.Geometry),
                    BikeAccessForward = new RoadSegmentDynamicAttributeValues<bool>(false, TestData.AddSegment1.Geometry),
                    PedestrianAccess = new RoadSegmentDynamicAttributeValues<bool>(false, TestData.AddSegment1.Geometry)
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
                    Geometry = BuildRoadSegmentGeometry(segment2Start, segment2End),
                    Status = RoadSegmentStatusV2.Gerealiseerd,
                    CarAccessBackward = new RoadSegmentDynamicAttributeValues<bool>(false, TestData.AddSegment2.Geometry),
                    CarAccessForward = new RoadSegmentDynamicAttributeValues<bool>(false, TestData.AddSegment2.Geometry),
                    BikeAccessBackward = new RoadSegmentDynamicAttributeValues<bool>(true, TestData.AddSegment2.Geometry),
                    BikeAccessForward = new RoadSegmentDynamicAttributeValues<bool>(true, TestData.AddSegment2.Geometry),
                    PedestrianAccess = new RoadSegmentDynamicAttributeValues<bool>(false, TestData.AddSegment2.Geometry)
                }).WithDynamicAttributePositionsOnEntireGeometryLength())
            )
            .When(changes => changes
                .Add(new ModifyRoadNodeChange
                {
                    RoadNodeId = TestData.Segment2StartNodeAdded.RoadNodeId,
                    Geometry = segment2NewLocationStart.ToRoadNodeGeometry()
                })
                .Add(new ModifyRoadNodeChange
                {
                    RoadNodeId = TestData.Segment2EndNodeAdded.RoadNodeId,
                    Geometry = segment2NewLocationEnd.ToRoadNodeGeometry()
                })
                .Add(new ModifyRoadSegmentChange
                {
                    RoadSegmentIdReference = new RoadSegmentIdReference(TestData.Segment2Added.RoadSegmentId),
                    Geometry = BuildRoadSegmentGeometry(segment2NewLocationStart, segment2NewLocationEnd)
                })
            )
            .Then((result, events) =>
            {
                result.Summary.GradeJunctions.Removed.Should().HaveCount(1);
                events.OfType<GradeJunctionWasRemoved>().Should().HaveCount(1);
            })
        );
    }

    [Fact]
    public Task GivenGradeJunction_WhenSegmentsGainTrafficOverlap_ThenGradeJunctionIsRemovedAndSummaryIsUpdated()
    {
        var segment1Start = new Point(0, 0);
        var segment1End = new Point(10, 0);
        var segment2Start = new Point(5, 5);
        var segment2End = new Point(5, -5);
        var segment2Geometry = BuildRoadSegmentGeometry(segment2Start, segment2End);

        return Run(scenario => scenario
            .Given(changes => changes
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
                    Status = RoadSegmentStatusV2.Gerealiseerd,
                    CarAccessBackward = new RoadSegmentDynamicAttributeValues<bool>(true, TestData.AddSegment1.Geometry),
                    CarAccessForward = new RoadSegmentDynamicAttributeValues<bool>(true, TestData.AddSegment1.Geometry),
                    BikeAccessBackward = new RoadSegmentDynamicAttributeValues<bool>(false, TestData.AddSegment1.Geometry),
                    BikeAccessForward = new RoadSegmentDynamicAttributeValues<bool>(false, TestData.AddSegment1.Geometry),
                    PedestrianAccess = new RoadSegmentDynamicAttributeValues<bool>(false, TestData.AddSegment1.Geometry)
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
                    Geometry = segment2Geometry,
                    Status = RoadSegmentStatusV2.Gerealiseerd,
                    CarAccessBackward = new RoadSegmentDynamicAttributeValues<bool>(false, TestData.AddSegment2.Geometry),
                    CarAccessForward = new RoadSegmentDynamicAttributeValues<bool>(false, TestData.AddSegment2.Geometry),
                    BikeAccessBackward = new RoadSegmentDynamicAttributeValues<bool>(true, TestData.AddSegment2.Geometry),
                    BikeAccessForward = new RoadSegmentDynamicAttributeValues<bool>(true, TestData.AddSegment2.Geometry),
                    PedestrianAccess = new RoadSegmentDynamicAttributeValues<bool>(false, TestData.AddSegment2.Geometry)
                }).WithDynamicAttributePositionsOnEntireGeometryLength())
            )
            .When(changes => changes
                .Add(new ModifyRoadSegmentChange
                {
                    RoadSegmentIdReference = new RoadSegmentIdReference(TestData.Segment2Added.RoadSegmentId),
                    CarAccessBackward = new RoadSegmentDynamicAttributeValues<bool>(true, segment2Geometry),
                    CarAccessForward = new RoadSegmentDynamicAttributeValues<bool>(true, segment2Geometry),
                    BikeAccessBackward = new RoadSegmentDynamicAttributeValues<bool>(false, segment2Geometry),
                    BikeAccessForward = new RoadSegmentDynamicAttributeValues<bool>(false, segment2Geometry),
                    PedestrianAccess = new RoadSegmentDynamicAttributeValues<bool>(false, segment2Geometry)
                })
                .Add(Fixture.Create<AddGradeSeparatedJunctionChange>() with
                {
                    LowerRoadSegmentId = TestData.Segment1Added.RoadSegmentId,
                    UpperRoadSegmentId = TestData.Segment2Added.RoadSegmentId,
                })
            )
            .Then((result, events) =>
            {
                result.Summary.GradeJunctions.Removed.Should().HaveCount(1);
                events.OfType<GradeJunctionWasRemoved>().Should().HaveCount(1);
            })
        );
    }

    [Fact]
    public Task GivenGradeJunction_WhenIntersectionMovedTooCloseToRoadNodes_ThenGradeJunctionIsRemovedAndSummaryIsUpdated()
    {
        var segment1Start = new Point(0, 0);
        var segment1End = new Point(10, 0);
        var segment2Start = new Point(5, 5);
        var segment2End = new Point(5, -5);

        var segment2NewLocationStart = new Point(0, 5);
        var segment2NewLocationEnd = new Point(0, -5);

        return Run(scenario => scenario
            .Given(changes => changes
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
                    Status = RoadSegmentStatusV2.Gerealiseerd,
                    CarAccessBackward = new RoadSegmentDynamicAttributeValues<bool>(true, TestData.AddSegment1.Geometry),
                    CarAccessForward = new RoadSegmentDynamicAttributeValues<bool>(true, TestData.AddSegment1.Geometry),
                    BikeAccessBackward = new RoadSegmentDynamicAttributeValues<bool>(false, TestData.AddSegment1.Geometry),
                    BikeAccessForward = new RoadSegmentDynamicAttributeValues<bool>(false, TestData.AddSegment1.Geometry),
                    PedestrianAccess = new RoadSegmentDynamicAttributeValues<bool>(false, TestData.AddSegment1.Geometry)
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
                    Geometry = BuildRoadSegmentGeometry(segment2Start, segment2End),
                    Status = RoadSegmentStatusV2.Gerealiseerd,
                    CarAccessBackward = new RoadSegmentDynamicAttributeValues<bool>(false, TestData.AddSegment2.Geometry),
                    CarAccessForward = new RoadSegmentDynamicAttributeValues<bool>(false, TestData.AddSegment2.Geometry),
                    BikeAccessBackward = new RoadSegmentDynamicAttributeValues<bool>(true, TestData.AddSegment2.Geometry),
                    BikeAccessForward = new RoadSegmentDynamicAttributeValues<bool>(true, TestData.AddSegment2.Geometry),
                    PedestrianAccess = new RoadSegmentDynamicAttributeValues<bool>(false, TestData.AddSegment2.Geometry)
                }).WithDynamicAttributePositionsOnEntireGeometryLength())
            )
            .When(changes => changes
                .Add(new ModifyRoadNodeChange
                {
                    RoadNodeId = TestData.Segment2StartNodeAdded.RoadNodeId,
                    Geometry = segment2NewLocationStart.ToRoadNodeGeometry()
                })
                .Add(new ModifyRoadNodeChange
                {
                    RoadNodeId = TestData.Segment2EndNodeAdded.RoadNodeId,
                    Geometry = segment2NewLocationEnd.ToRoadNodeGeometry()
                })
                .Add(new ModifyRoadSegmentChange
                {
                    RoadSegmentIdReference = new RoadSegmentIdReference(TestData.Segment2Added.RoadSegmentId),
                    Geometry = BuildRoadSegmentGeometry(segment2NewLocationStart, segment2NewLocationEnd)
                })
            )
            .Then((result, events) =>
            {
                result.Summary.GradeJunctions.Removed.Should().HaveCount(1);
                events.OfType<GradeJunctionWasRemoved>().Should().HaveCount(1);
            })
        );
    }
}
