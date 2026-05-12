namespace RoadRegistry.Tests.AggregateTests.RoadSegment.MergeRoadSegment;

using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.RoadNode.Events.V2;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.Tests.AggregateTests.Framework;

public class ScopedRoadNetworkTests : RoadNetworkTestBase
{
    [Fact]
    public Task ThenSummaryIsUpdated()
    {
        return Run(scenario => scenario
            .Given(given => given
                .Add(TestData.AddSegment1StartNode)
                .Add(TestData.AddSegment1EndNode)
                .Add(TestData.AddSegment1))
            .WhenMigrate(changes => changes
                .Add(new ModifyRoadSegmentChange
                {
                    RoadSegmentIdReference = new RoadSegmentIdReference(TestData.Segment1Added.RoadSegmentId),
                    Geometry = TestData.AddSegment1.Geometry,
                    GeometryDrawMethod = TestData.AddSegment1.GeometryDrawMethod,
                    AccessRestriction = TestData.AddSegment1.AccessRestriction,
                    Category = TestData.AddSegment1.Category,
                    Morphology = TestData.AddSegment1.Morphology,
                    Status = TestData.AddSegment1.Status,
                    StreetNameId = TestData.AddSegment1.StreetNameId,
                    MaintenanceAuthorityId = TestData.AddSegment1.MaintenanceAuthorityId,
                    SurfaceType = TestData.AddSegment1.SurfaceType,
                    CarAccessForward = TestData.AddSegment1.CarAccessForward,
                    CarAccessBackward = TestData.AddSegment1.CarAccessBackward,
                    BikeAccessForward = TestData.AddSegment1.BikeAccessForward,
                    BikeAccessBackward = TestData.AddSegment1.BikeAccessBackward,
                    PedestrianAccess = TestData.AddSegment1.PedestrianAccess
                })
            )
            .Then((result, events) =>
            {
                result.Summary.RoadSegments.Modified.Should().HaveCount(1);
            })
        );
    }

    [Fact]
    public Task WhenGeometryEqualityIsLessThanThreshold_ThenMergedWithNewRoadSegmentId()
    {
        var point1 = new Point(600000, 600000).WithSrid(WellknownSrids.Lambert08);
        var point2 = new Point(600050, 600000).WithSrid(WellknownSrids.Lambert08);
        var point3 = new Point(600100, 600000).WithSrid(WellknownSrids.Lambert08);

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
                }).WithDynamicAttributePositionsOnEntireGeometryLength())
            )
            .Then((_, events) =>
            {
                var roadNodeWasRemoved = events
                    .OfType<RoadNodeWasRemoved>()
                    .SingleOrDefault(x => x.RoadNodeId == 2);
                roadNodeWasRemoved.Should().NotBeNull();

                events.OfType<RoadSegmentWasRetiredBecauseOfMerger>().Should().HaveCount(2);
                var roadSegment1WasRetiredBecauseOfMerger = events
                    .OfType<RoadSegmentWasRetiredBecauseOfMerger>()
                    .SingleOrDefault(x => x.RoadSegmentId == new RoadSegmentId(1));
                roadSegment1WasRetiredBecauseOfMerger.Should().NotBeNull();

                var roadSegment2WasRetiredBecauseOfMerger = events
                    .OfType<RoadSegmentWasRetiredBecauseOfMerger>()
                    .SingleOrDefault(x => x.RoadSegmentId == new RoadSegmentId(2));
                roadSegment2WasRetiredBecauseOfMerger.Should().NotBeNull();

                events.OfType<RoadSegmentWasMerged>().Should().HaveCount(1);
                var roadSegmentWasMerged = events
                    .OfType<RoadSegmentWasMerged>()
                    .SingleOrDefault(x => x.RoadSegmentId == new RoadSegmentId(3));
                roadSegmentWasMerged.Should().NotBeNull();
            })
        );
    }

    [Fact]
    public Task WhenGeometryEqualityIsMoreThanThreshold_ThenLongestRoadSegmentIsModified()
    {

        var point1 = new Point(600000, 600000).WithSrid(WellknownSrids.Lambert08);
        var point2 = new Point(600075, 600000).WithSrid(WellknownSrids.Lambert08);
        var point3 = new Point(600100, 600000).WithSrid(WellknownSrids.Lambert08);

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
                }).WithDynamicAttributePositionsOnEntireGeometryLength())
            )
            .Then((_, events) =>
            {
                var roadNodeWasRemoved = events
                    .OfType<RoadNodeWasRemoved>()
                    .SingleOrDefault(x => x.RoadNodeId == 2);
                roadNodeWasRemoved.Should().NotBeNull();

                events.OfType<RoadSegmentWasMerged>().Should().HaveCount(1);
                var roadSegment1WasMerged = events
                    .OfType<RoadSegmentWasMerged>()
                    .SingleOrDefault(x => x.RoadSegmentId == new RoadSegmentId(1));
                roadSegment1WasMerged.Should().NotBeNull();

                events.OfType<RoadSegmentWasRetiredBecauseOfMerger>().Should().HaveCount(1);
                var roadSegment2WasRetiredBecauseOfMerger = events
                    .OfType<RoadSegmentWasRetiredBecauseOfMerger>()
                    .SingleOrDefault(x => x.RoadSegmentId == new RoadSegmentId(2));
                roadSegment2WasRetiredBecauseOfMerger.Should().NotBeNull();
            })
        );
    }
}
