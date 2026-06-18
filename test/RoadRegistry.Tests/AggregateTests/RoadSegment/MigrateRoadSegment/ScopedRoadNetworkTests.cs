namespace RoadRegistry.Tests.AggregateTests.RoadSegment.MigrateRoadSegment;

using AutoFixture;
using FluentAssertions;
using RoadRegistry.RoadNetwork.Schema;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.Tests.AggregateTests.Framework;
using RoadNode = RoadRegistry.RoadNode.RoadNode;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

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
                    GeometryDrawMethod = TestData.AddSegment1.GeometryDrawMethod == RoadSegmentGeometryDrawMethodV2.Ingemeten
                        ? RoadSegmentGeometryDrawMethodV2.Ingeschetst
                        : RoadSegmentGeometryDrawMethodV2.Ingemeten,
                    AccessRestriction = TestData.AddSegment1.AccessRestriction,
                    Category = TestData.AddSegment1.Category,
                    Morphology = TestData.AddSegment1.Morphology,
                    Status = TestData.AddSegment1.Status,
                    StreetNameId = TestData.AddSegment1.StreetNameId,
                    MaintenanceAuthorityId = TestData.AddSegment1.MaintenanceAuthorityId,
                    SurfaceType = TestData.AddSegment1.SurfaceType,
                    CarTrafficDirection = TestData.AddSegment1.CarTrafficDirection,
                    BikeTrafficDirection = TestData.AddSegment1.BikeTrafficDirection,
                    PedestrianTrafficDirection = TestData.AddSegment1.PedestrianTrafficDirection
                })
            )
            .Then((result, events) =>
            {
                result.Summary.RoadSegments.Modified.Should().HaveCount(1);
            })
        );
    }

    [Fact]
    public void WithEuropeanRoadNumbers_ThenIncludedInEvent()
    {
        var linkedNumber = Fixture.Create<EuropeanRoadNumber>();
        var removedNumber = Fixture.CreateWhichIsDifferentThan(linkedNumber);

        // v1 situation
        var roadNetwork = new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(),
            roadNodes:
            [
                RoadNode.CreateForMigration(TestData.Segment1StartNodeAdded.RoadNodeId, TestData.Segment1StartNodeAdded.Geometry),
                RoadNode.CreateForMigration(TestData.Segment1EndNodeAdded.RoadNodeId, TestData.Segment1EndNodeAdded.Geometry),
            ],
            roadSegments:
            [
                RoadSegment.CreateForMigration(TestData.Segment1Added.RoadSegmentId, TestData.Segment1Added.Geometry, TestData.Segment1Added.Status, TestData.Segment1StartNodeAdded.RoadNodeId, TestData.Segment1EndNodeAdded.RoadNodeId),
            ]);

        // Act
        var result = roadNetwork.Migrate(RoadNetworkChanges.Start()
                .WithProvenance(new FakeProvenance())
                .Add(new ModifyRoadNodeChange
                {
                    RoadNodeId = TestData.Segment1StartNodeAdded.RoadNodeId,
                    Geometry = TestData.Segment1StartNodeAdded.Geometry,
                    Grensknoop = false
                })
                .Add(new ModifyRoadNodeChange
                {
                    RoadNodeId = TestData.Segment1EndNodeAdded.RoadNodeId,
                    Geometry = TestData.Segment1EndNodeAdded.Geometry,
                    Grensknoop = false
                })
                .Add(new ModifyRoadSegmentChange
                {
                    RoadSegmentIdReference = new RoadSegmentIdReference(TestData.Segment1Added.RoadSegmentId),
                    Geometry = TestData.Segment1Added.Geometry,
                    GeometryDrawMethod = TestData.Segment1Added.GeometryDrawMethod == RoadSegmentGeometryDrawMethodV2.Ingemeten
                        ? RoadSegmentGeometryDrawMethodV2.Ingeschetst
                        : RoadSegmentGeometryDrawMethodV2.Ingemeten,
                    AccessRestriction = TestData.Segment1Added.AccessRestriction,
                    Category = TestData.Segment1Added.Category,
                    Morphology = TestData.Segment1Added.Morphology,
                    Status = TestData.Segment1Added.Status,
                    StreetNameId = TestData.Segment1Added.StreetNameId,
                    MaintenanceAuthorityId = TestData.Segment1Added.MaintenanceAuthorityId,
                    SurfaceType = TestData.Segment1Added.SurfaceType,
                    CarTrafficDirection = TestData.Segment1Added.CarTrafficDirection,
                    BikeTrafficDirection = TestData.Segment1Added.BikeTrafficDirection,
                    PedestrianTrafficDirection = TestData.Segment1Added.PedestrianTrafficDirection
                })
                .Add(new AddRoadSegmentToEuropeanRoadChange
                {
                    RoadSegmentId = TestData.Segment1Added.RoadSegmentId,
                    Number = linkedNumber
                })
                .Add(new AddRoadSegmentToEuropeanRoadChange
                {
                    RoadSegmentId = TestData.Segment1Added.RoadSegmentId,
                    Number = removedNumber
                })
                .Add(new RemoveRoadSegmentFromEuropeanRoadChange
                {
                    RoadSegmentId = TestData.Segment1Added.RoadSegmentId,
                    Number = removedNumber
                }),
            TestData.Fixture.Create<DownloadId>(),
            new InMemoryRoadNetworkIdGenerator());

        result.Problems.Should().HaveNoError();

        var events = roadNetwork.RoadSegments[TestData.Segment1Added.RoadSegmentId].GetChanges();
        var roadSegmentWasMigrated = events.OfType<RoadSegmentWasMigrated>().Single();

        roadSegmentWasMigrated.EuropeanRoadNumbers.Should().HaveCount(1);
        roadSegmentWasMigrated.EuropeanRoadNumbers.Should().Contain(linkedNumber);
        roadSegmentWasMigrated.NationalRoadNumbers.Should().BeEmpty();
    }

    [Fact]
    public void WithNationalRoadNumbers_ThenIncludedInEvent()
    {
        var linkedNumber = Fixture.Create<NationalRoadNumber>();
        var removedNumber = Fixture.CreateWhichIsDifferentThan(linkedNumber);

        // v1 situation
        var roadNetwork = new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(),
            roadNodes:
            [
                RoadNode.CreateForMigration(TestData.Segment1StartNodeAdded.RoadNodeId, TestData.Segment1StartNodeAdded.Geometry),
                RoadNode.CreateForMigration(TestData.Segment1EndNodeAdded.RoadNodeId, TestData.Segment1EndNodeAdded.Geometry),
            ],
            roadSegments:
            [
                RoadSegment.CreateForMigration(TestData.Segment1Added.RoadSegmentId, TestData.Segment1Added.Geometry, TestData.Segment1Added.Status, TestData.Segment1StartNodeAdded.RoadNodeId, TestData.Segment1EndNodeAdded.RoadNodeId),
            ]);

        // Act
        var result = roadNetwork.Migrate(RoadNetworkChanges.Start()
                .WithProvenance(new FakeProvenance())
                .Add(new ModifyRoadNodeChange
                {
                    RoadNodeId = TestData.Segment1StartNodeAdded.RoadNodeId,
                    Geometry = TestData.Segment1StartNodeAdded.Geometry,
                    Grensknoop = false
                })
                .Add(new ModifyRoadNodeChange
                {
                    RoadNodeId = TestData.Segment1EndNodeAdded.RoadNodeId,
                    Geometry = TestData.Segment1EndNodeAdded.Geometry,
                    Grensknoop = false
                })
                .Add(new ModifyRoadSegmentChange
                {
                    RoadSegmentIdReference = new RoadSegmentIdReference(TestData.Segment1Added.RoadSegmentId),
                    Geometry = TestData.Segment1Added.Geometry,
                    GeometryDrawMethod = TestData.Segment1Added.GeometryDrawMethod == RoadSegmentGeometryDrawMethodV2.Ingemeten
                        ? RoadSegmentGeometryDrawMethodV2.Ingeschetst
                        : RoadSegmentGeometryDrawMethodV2.Ingemeten,
                    AccessRestriction = TestData.Segment1Added.AccessRestriction,
                    Category = TestData.Segment1Added.Category,
                    Morphology = TestData.Segment1Added.Morphology,
                    Status = TestData.Segment1Added.Status,
                    StreetNameId = TestData.Segment1Added.StreetNameId,
                    MaintenanceAuthorityId = TestData.Segment1Added.MaintenanceAuthorityId,
                    SurfaceType = TestData.Segment1Added.SurfaceType,
                    CarTrafficDirection = TestData.Segment1Added.CarTrafficDirection,
                    BikeTrafficDirection = TestData.Segment1Added.BikeTrafficDirection,
                    PedestrianTrafficDirection = TestData.Segment1Added.PedestrianTrafficDirection
                })
                .Add(new AddRoadSegmentToNationalRoadChange
                {
                    RoadSegmentId = TestData.Segment1Added.RoadSegmentId,
                    Number = linkedNumber
                })
                .Add(new AddRoadSegmentToNationalRoadChange
                {
                    RoadSegmentId = TestData.Segment1Added.RoadSegmentId,
                    Number = removedNumber
                })
                .Add(new RemoveRoadSegmentFromNationalRoadChange
                {
                    RoadSegmentId = TestData.Segment1Added.RoadSegmentId,
                    Number = removedNumber
                }),
            TestData.Fixture.Create<DownloadId>(),
            new InMemoryRoadNetworkIdGenerator());

        result.Problems.Should().HaveNoError();

        var events = roadNetwork.RoadSegments[TestData.Segment1Added.RoadSegmentId].GetChanges();
        var roadSegmentWasMigrated = events.OfType<RoadSegmentWasMigrated>().Single();

        roadSegmentWasMigrated.NationalRoadNumbers.Should().HaveCount(1);
        roadSegmentWasMigrated.NationalRoadNumbers.Should().Contain(linkedNumber);
        roadSegmentWasMigrated.EuropeanRoadNumbers.Should().BeEmpty();
    }
}
