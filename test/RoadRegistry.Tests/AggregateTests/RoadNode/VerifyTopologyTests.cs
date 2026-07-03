namespace RoadRegistry.Tests.AggregateTests.RoadNode;

using AutoFixture;
using RoadRegistry.Extensions;
using FluentAssertions;
using Framework;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using RoadRegistry.RoadNetwork.Schema;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadNode.Events.V2;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.ValueObjects;
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
            .Then((result, events) => { events.Should().ContainItemsAssignableTo<RoadNodeTypeWasChanged>(); })
        );
    }

    [Fact]
    public Task WhenGeometryIsExactlyEqualToOtherNodeGeometry_ThenError()
    {
        return Run(scenario => scenario
            .Given(g => g)
            .When(changes => changes
                .Add(TestData.AddSegment1StartNode)
                .Add(TestData.AddSegment1EndNode with
                {
                    Geometry = TestData.AddSegment1StartNode.Geometry
                })
            )
            .ThenContainsProblems(new Error("RoadNodeGeometryTaken",
                new ProblemParameter("ByOtherNode", TestData.AddSegment1StartNode.TemporaryId.ToString()),
                new ProblemParameter("WegknoopId", TestData.AddSegment1EndNode.TemporaryId.ToString())
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
        var point2 = new Point(600011, 600000).WithSrid(WellknownSrids.Lambert08);
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

                var roadSegmentWasAdded = events
                    .OfType<RoadSegmentWasAdded>()
                    .SingleOrDefault(x => x.RoadSegmentId == new RoadSegmentId(1));
                roadSegmentWasAdded.Should().NotBeNull();

                var roadSegmentWasMerged = events
                    .OfType<RoadSegmentWasMerged>()
                    .SingleOrDefault(x => x.OtherRoadSegmentId == new RoadSegmentId(2));
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
            roadNodes:
            [
                RoadNode.CreateForMigration(TestData.Segment1StartNodeAdded.RoadNodeId, RoadNodeGeometry.Create(point1)),
                RoadNode.CreateForMigration(TestData.Segment1EndNodeAdded.RoadNodeId, RoadNodeGeometry.Create(point2)),
                RoadNode.CreateForMigration(TestData.Segment2EndNodeAdded.RoadNodeId, RoadNodeGeometry.Create(point3)),
            ],
            roadSegments:
            [
                RoadSegment.CreateForMigration(TestData.Segment1Added.RoadSegmentId, BuildRoadSegmentGeometry(point1, point2), TestData.Segment1Added.Status, TestData.Segment1StartNodeAdded.RoadNodeId, TestData.Segment1EndNodeAdded.RoadNodeId),
                RoadSegment.CreateForMigration(TestData.Segment2Added.RoadSegmentId, BuildRoadSegmentGeometry(point2, point3), TestData.Segment1Added.Status, TestData.Segment1EndNodeAdded.RoadNodeId, TestData.Segment2EndNodeAdded.RoadNodeId),
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
                    CarTrafficDirection = TestData.Segment2Added.CarTrafficDirection,
                    BikeTrafficDirection = TestData.Segment2Added.BikeTrafficDirection,
                    PedestrianTrafficDirection = TestData.Segment2Added.PedestrianTrafficDirection
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
        var point2 = new Point(600011, 600000);
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

                var roadSegmentWasAdded = events
                    .OfType<RoadSegmentWasAdded>()
                    .SingleOrDefault(x => x.RoadSegmentId == new RoadSegmentId(1));
                roadSegmentWasAdded.Should().NotBeNull();

                var roadSegmentWasMerged = events
                    .OfType<RoadSegmentWasMerged>()
                    .SingleOrDefault(x => x.OtherRoadSegmentId == new RoadSegmentId(2));
                roadSegmentWasMerged.Should().NotBeNull();
            })
        );
    }

    [Fact]
    public void WhenDetectType_WithTwoSegmentsConnectedAndNodeIsNoLongerNeeded_ThenSegmentsMerged()
    {
        // Arrange
        var roadSegment1 = JsonConvert.DeserializeObject<RoadSegment>("{\"roadSegmentId\":1,\"geometry\":{\"srid\":3812,\"wkt\":\"MULTILINESTRING ((527249.44 699840.08, 527248.19 699822.00, 527244.40 699760.75, 527242.22 699725.58))\"},\"status\":\"Gerealiseerd\",\"startNodeId\":1,\"endNodeId\":2,\"attributes\":{\"geometryDrawMethod\":\"Ingeschetst\",\"accessRestriction\":[{\"coverage\":{\"from\":0.0,\"to\":114.72583022308163},\"value\":\"OpenbareWeg\"}],\"category\":[{\"coverage\":{\"from\":0.0,\"to\":114.72583022308163},\"value\":\"NietVanToepassing\"}],\"morphology\":[{\"coverage\":{\"from\":0.0,\"to\":114.72583022308163},\"value\":\"Parallelweg\"}],\"streetNameId\":[{\"side\":1,\"coverage\":{\"from\":0.0,\"to\":114.72583022308163},\"value\":-9},{\"side\":2,\"coverage\":{\"from\":0.0,\"to\":114.72583022308163},\"value\":-9}],\"maintenanceAuthorityId\":[{\"side\":1,\"coverage\":{\"from\":0.0,\"to\":114.72583022308163},\"value\":\"38014\"},{\"side\":2,\"coverage\":{\"from\":0.0,\"to\":114.72583022308163},\"value\":\"38014\"}],\"surfaceType\":[{\"coverage\":{\"from\":0.0,\"to\":114.72583022308163},\"value\":\"Halfverhard\"}],\"carTrafficDirection\":[{\"coverage\":{\"from\":0.0,\"to\":114.72583022308163},\"value\":\"Both\"}],\"bikeTrafficDirection\":[{\"coverage\":{\"from\":0.0,\"to\":114.72583022308163},\"value\":\"None\"}],\"pedestrianTrafficDirection\":[{\"coverage\":{\"from\":0.0,\"to\":114.72583022308163},\"value\":\"None\"}],\"europeanRoadNumbers\":[],\"nationalRoadNumbers\":[]},\"mergedRoadSegmentId\":null,\"isRemoved\":false}", WellKnownJsonSerializerSettings.Marten);
        var roadSegment2 = JsonConvert.DeserializeObject<RoadSegment>("{\"roadSegmentId\":2,\"geometry\":{\"srid\":3812,\"wkt\":\"MULTILINESTRING ((527249.44 699840.08, 527253.45 699899.58, 527252.17 699908.79, 527251.64 699913.13, 527251.75 699918.63, 527251.85 699922.23, 527251.75 699925.94, 527251.43 699928.79, 527249.62 699940.44, 527248.67 699947.72, 527249.62 699955.58, 527254.02 699991.78, 527254.37 699994.72))\"},\"status\":\"Gerealiseerd\",\"startNodeId\":1,\"endNodeId\":3,\"attributes\":{\"geometryDrawMethod\":\"Ingeschetst\",\"accessRestriction\":[{\"coverage\":{\"from\":0.0,\"to\":108.11739493423526},\"value\":\"OpenbareWeg\"},{\"coverage\":{\"from\":108.11739493423526,\"to\":155.4605154715142},\"value\":\"OpenbareWeg\"}],\"category\":[{\"coverage\":{\"from\":0.0,\"to\":108.11739493423526},\"value\":\"NietVanToepassing\"},{\"coverage\":{\"from\":108.11739493423526,\"to\":155.4605154715142},\"value\":\"LokaleErftoegangsweg\"}],\"morphology\":[{\"coverage\":{\"from\":0.0,\"to\":108.11739493423526},\"value\":\"Parallelweg\"},{\"coverage\":{\"from\":108.11739493423526,\"to\":155.4605154715142},\"value\":\"WegBestaandeUit1Rijbaan\"}],\"streetNameId\":[{\"side\":1,\"coverage\":{\"from\":0.0,\"to\":108.11739493423526},\"value\":-9},{\"side\":2,\"coverage\":{\"from\":0.0,\"to\":108.11739493423526},\"value\":-9},{\"side\":1,\"coverage\":{\"from\":108.11739493423526,\"to\":155.4605154715142},\"value\":-9},{\"side\":2,\"coverage\":{\"from\":108.11739493423526,\"to\":155.4605154715142},\"value\":-9}],\"maintenanceAuthorityId\":[{\"side\":1,\"coverage\":{\"from\":0.0,\"to\":108.11739493423526},\"value\":\"38014\"},{\"side\":2,\"coverage\":{\"from\":0.0,\"to\":108.11739493423526},\"value\":\"38014\"},{\"side\":1,\"coverage\":{\"from\":108.11739493423526,\"to\":155.4605154715142},\"value\":\"38014\"},{\"side\":2,\"coverage\":{\"from\":108.11739493423526,\"to\":155.4605154715142},\"value\":\"38014\"}],\"surfaceType\":[{\"coverage\":{\"from\":0.0,\"to\":108.11739493423526},\"value\":\"Halfverhard\"},{\"coverage\":{\"from\":108.11739493423526,\"to\":155.4605154715142},\"value\":\"Halfverhard\"}],\"carTrafficDirection\":[{\"coverage\":{\"from\":0.0,\"to\":108.11739493423526},\"value\":\"Both\"},{\"coverage\":{\"from\":108.11739493423526,\"to\":155.4605154715142},\"value\":\"Both\"}],\"bikeTrafficDirection\":[{\"coverage\":{\"from\":0.0,\"to\":108.11739493423526},\"value\":\"None\"},{\"coverage\":{\"from\":108.11739493423526,\"to\":155.4605154715142},\"value\":\"None\"}],\"pedestrianTrafficDirection\":[{\"coverage\":{\"from\":0.0,\"to\":108.11739493423526},\"value\":\"None\"},{\"coverage\":{\"from\":108.11739493423526,\"to\":155.4605154715142},\"value\":\"None\"}],\"europeanRoadNumbers\":[],\"nationalRoadNumbers\":[]},\"mergedRoadSegmentId\":null,\"isRemoved\":false}", WellKnownJsonSerializerSettings.Marten);
        var roadSegment3 = RoadSegment.CreateForMigration(new RoadSegmentId(3), new RoadSegmentGeometry(3812, "MULTILINESTRING ((527249.44 699840.08, 527253.45 699899.58))"), RoadSegmentStatusV2.Gerealiseerd, new RoadNodeId(1), new RoadNodeId(4));

        var roadNetwork = new ScopedRoadNetwork(
            Fixture.Create<ScopedRoadNetworkId>(),
            [
                RoadNode.CreateForMigration(new RoadNodeId(1), RoadNodeGeometry.Create(roadSegment1.Geometry.Value.GetSingleLineString().StartPoint)),
                RoadNode.CreateForMigration(new RoadNodeId(2), RoadNodeGeometry.Create(roadSegment1.Geometry.Value.GetSingleLineString().EndPoint)),
                RoadNode.CreateForMigration(new RoadNodeId(3), RoadNodeGeometry.Create(roadSegment2.Geometry.Value.GetSingleLineString().EndPoint)),
                RoadNode.CreateForMigration(new RoadNodeId(4), RoadNodeGeometry.Create(roadSegment3.Geometry.Value.GetSingleLineString().EndPoint)),
            ],
            [roadSegment1, roadSegment2, roadSegment3]
        );

        // Act
        var result = roadNetwork.Migrate(RoadNetworkChanges.Start()
                .WithProvenance(new FakeProvenance())
                .Add(new RemoveRoadSegmentChange
                {
                    RoadSegmentId = roadSegment3.RoadSegmentId
                })
                .Add(new RemoveRoadNodeChange
                {
                    RoadNodeId = new RoadNodeId(4)
                }),
            TestData.Fixture.Create<DownloadId>(),
            new InMemoryRoadNetworkIdGenerator(initialValue: 100));

        result.Problems.Should().BeEmpty();

        roadNetwork.RoadNodes[new RoadNodeId(1)].IsRemoved.Should().BeTrue();
        roadNetwork.RoadNodes[new RoadNodeId(4)].IsRemoved.Should().BeTrue();
        roadNetwork.RoadSegments[new RoadSegmentId(1)].Status.Should().Be(RoadSegmentStatusV2.Gehistoreerd);
        roadNetwork.RoadSegments[new RoadSegmentId(1)].MergedRoadSegmentId.Should().Be(new RoadSegmentId(100));
        roadNetwork.RoadSegments[new RoadSegmentId(2)].Status.Should().Be(RoadSegmentStatusV2.Gehistoreerd);
        roadNetwork.RoadSegments[new RoadSegmentId(2)].MergedRoadSegmentId.Should().Be(new RoadSegmentId(100));
        roadNetwork.RoadSegments[new RoadSegmentId(100)].GetChanges().Should().ContainItemsAssignableTo<RoadSegmentWasMerged>();
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
