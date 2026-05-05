namespace RoadRegistry.Tests.AggregateTests.RoadNode;

using AutoFixture;
using RoadRegistry.Extensions;
using FluentAssertions;
using Framework;
using NetTopologySuite.Geometries;
using RoadRegistry.RoadNetwork.Schema;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadNode.Events.V2;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using ValueObjects.Problems;
using RoadNode = RoadRegistry.RoadNode.RoadNode;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

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
                new ProblemParameter("OtherSegmentId", TestData.Segment1Added.RoadSegmentId.ToString()),
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
    public Task WhenDetectType_WithTwoDifferentSegmentsConnectedAndNodeIsNotGrensknoop_ThenSegmentsMerged()
    {
        var point1 = new Point(600000, 600000).WithSrid(WellknownSrids.Lambert08);
        var point2 = new Point(600010, 600000).WithSrid(WellknownSrids.Lambert08);
        var point3 = new Point(600020, 600000).WithSrid(WellknownSrids.Lambert08);

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
                var roadNodeWasRemoved = events
                    .OfType<RoadNodeWasRemoved>()
                    .SingleOrDefault(x => x.RoadNodeId == 2);
                roadNodeWasRemoved.Should().NotBeNull();

                var roadSegmentWasMerged = events
                    .OfType<RoadSegmentWasMerged>()
                    .SingleOrDefault(x => x.OriginalIds.SequenceEqual([new RoadSegmentId(1), new RoadSegmentId(2)]));
                roadSegmentWasMerged.Should().NotBeNull();
            })
        );
    }

    [Fact]
    public Task WhenDetectType_WithTwoIdenticalSegmentsConnectedAndNodeIsGrensknoop_ThenTypeIsValidatieknoop()
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
                    .SingleOrDefault(x => x.RoadNodeId == TestData.Segment1EndNodeAdded.RoadNodeId && x.Type == RoadNodeTypeV2.Validatieknoop);
                wasChanged.Should().NotBeNull();
            })
        );
    }

    [Fact]
    public void WhenDetectType_WithTwoSegmentsWhereOneIsV1_ThenValidatieknoop()
    {
        var point1 = new Point(600000, 600000);
        var point2 = new Point(600010, 600000);
        var point3 = new Point(600020, 600000);

        // v1 situation
        var roadNetwork = new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(),
            roadNodes: [
                RoadNode.CreateForMigration(TestData.Segment1StartNodeAdded.RoadNodeId, RoadNodeGeometry.Create(point1)),
                RoadNode.CreateForMigration(TestData.Segment1EndNodeAdded.RoadNodeId, RoadNodeGeometry.Create(point2)),
                RoadNode.CreateForMigration(TestData.Segment2EndNodeAdded.RoadNodeId, RoadNodeGeometry.Create(point3)),
            ],
            roadSegments: [
                RoadSegment.CreateForMigration(TestData.Segment1Added.RoadSegmentId, BuildRoadSegmentGeometry(point1, point2), TestData.Segment1StartNodeAdded.RoadNodeId, TestData.Segment1EndNodeAdded.RoadNodeId),
                RoadSegment.CreateForMigration(TestData.Segment2Added.RoadSegmentId, BuildRoadSegmentGeometry(point2, point3), TestData.Segment1EndNodeAdded.RoadNodeId, TestData.Segment2EndNodeAdded.RoadNodeId),
            ]);

        // Act
        var result = roadNetwork.Migrate(RoadNetworkChanges.Start()
                .WithProvenance(new FakeProvenance())
                .Add(new ModifyRoadNodeChange
                {
                    RoadNodeId = TestData.Segment1EndNodeAdded.RoadNodeId,
                    Geometry = RoadNodeGeometry.Create(point2),
                    Grensknoop = false,
                })
                .Add(new ModifyRoadNodeChange
                {
                    RoadNodeId = TestData.Segment2EndNodeAdded.RoadNodeId,
                    Geometry = RoadNodeGeometry.Create(point3),
                    Grensknoop = false
                })
                .Add((new ModifyRoadSegmentChange
                {
                    RoadSegmentIdReference = new RoadSegmentIdReference(TestData.Segment2Added.RoadSegmentId),
                    Geometry = BuildRoadSegmentGeometry(point2, point3),
                    GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Ingemeten,
                    Status = RoadSegmentStatusV2.Gerealiseerd,
                    AccessRestriction = TestData.Segment2Added.AccessRestriction,
                    Category = TestData.Segment2Added.Category,
                    MaintenanceAuthorityId = TestData.Segment2Added.MaintenanceAuthorityId,
                    Morphology = TestData.Segment2Added.Morphology,
                    StreetNameId = TestData.Segment2Added.StreetNameId,
                    SurfaceType = TestData.Segment2Added.SurfaceType,
                    CarAccessBackward = TestData.Segment2Added.CarAccessBackward,
                    CarAccessForward = TestData.Segment2Added.CarAccessForward,
                    BikeAccessBackward = TestData.Segment2Added.BikeAccessBackward,
                    BikeAccessForward = TestData.Segment2Added.BikeAccessForward,
                    PedestrianAccess = TestData.Segment2Added.PedestrianAccess
                }).WithDynamicAttributePositionsOnEntireGeometryLength()),
            TestData.Fixture.Create<DownloadId>(),
            new InMemoryRoadNetworkIdGenerator());

        // Assert
        result.Problems.Should().BeEmpty();

        roadNetwork.RoadNodes[new RoadNodeId(2)].Type.Should().Be(RoadNodeTypeV2.Validatieknoop);
        roadNetwork.RoadNodes[new RoadNodeId(4)].Type.Should().Be(RoadNodeTypeV2.Eindknoop);
    }

    [Fact]
    public Task WhenDetectType_WithTwoIdenticalSegmentsConnectedAndNodeIsNotGrensknoop_ThenSegmentsMerged()
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
            .Then((_, events) =>
            {
                var roadNodeWasRemoved = events
                    .OfType<RoadNodeWasRemoved>()
                    .SingleOrDefault(x => x.RoadNodeId == 2);
                roadNodeWasRemoved.Should().NotBeNull();

                var roadSegmentWasMerged = events
                    .OfType<RoadSegmentWasMerged>()
                    .SingleOrDefault(x => x.OriginalIds.SequenceEqual([new RoadSegmentId(1), new RoadSegmentId(2)]));
                roadSegmentWasMerged.Should().NotBeNull();
            })
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
