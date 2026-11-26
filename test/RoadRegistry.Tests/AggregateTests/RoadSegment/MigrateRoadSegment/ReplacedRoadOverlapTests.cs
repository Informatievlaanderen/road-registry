namespace RoadRegistry.Tests.AggregateTests.RoadSegment.MigrateRoadSegment;

using AutoFixture;
using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.RoadNetwork.Schema;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.Tests.AggregateTests.Framework;
using RoadRegistry.ValueObjects;
using RoadNode = RoadRegistry.RoadNode.RoadNode;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

// Real coordinates from an inwinning upload: the road was re-measured and arrives as a road drawn anew, with the
// road it replaces removed in the same upload. The two run a metre apart in the middle but share their start and
// their last two vertices exactly, so they are well within reach of the 'ligt te dicht bij' rule.
//
// The two cases are pinned together because only the second is a problem to report. An upload was refused over
// this pair although the road it replaces should have been removed along with it: the extract it was compared
// against had been read only halfway, so that road was never read, never matched and never removed. That is
// handled where the extract is read; what the rule itself makes of it is what these two say.
public class ReplacedRoadOverlapTests : RoadNetworkTestBase
{
    private static readonly Coordinate Start = new(662436.110, 713313.470);
    private static readonly Coordinate End = new(662437.890, 713373.310);

    private static readonly RoadNodeId StartNodeId = new(1);
    private static readonly RoadNodeId EndNodeId = new(2);

    private static readonly RoadSegmentId ExistingSegmentId = new(1294377);

    private static MultiLineString ExistingRoad => Line(
        Start,
        new Coordinate(662428.320, 713329.260),
        new Coordinate(662410.930, 713362.720),
        new Coordinate(662435.510, 713372.250),
        End);

    private static MultiLineString UploadedRoad => Line(
        Start,
        new Coordinate(662433.230, 713319.820),
        new Coordinate(662430.559, 713325.707),
        new Coordinate(662413.810, 713362.620),
        new Coordinate(662435.510, 713372.250),
        End);

    [Fact]
    public void WhenTheRoadItReplacesIsRemovedInTheSameUpload_ThenItIsNotTooClose()
    {
        var roadNetwork = BuildNetwork();

        var result = roadNetwork.Migrate(Upload(), null, IdGenerator());

        result.Problems.Select(x => x.Reason).Should().BeEmpty();
    }

    [Fact]
    public void WhenTheRoadItReplacesStays_ThenItIsTooClose()
    {
        var roadNetwork = BuildNetwork();

        var result = roadNetwork.Migrate(UploadWithoutTheRemoval(), null, IdGenerator());

        result.Problems.Select(x => x.Reason).Should().Contain("RoadSegmentPartiallyOverlapsWithAnotherRoadSegment");
    }

    private ScopedRoadNetwork BuildNetwork()
    {
        return new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(),
            roadNodes:
            [
                RoadNode.CreateForMigration(StartNodeId, Node(Start)),
                RoadNode.CreateForMigration(EndNodeId, Node(End))
            ],
            roadSegments:
            [
                BuildCollectedRoadSegment(ExistingSegmentId, ExistingRoad, StartNodeId, EndNodeId)
            ]).WithoutChanges();
    }

    private RoadNetworkChanges Upload()
    {
        return UploadWithoutTheRemoval().Add(new RemoveRoadSegmentChange { RoadSegmentId = ExistingSegmentId });
    }

    private RoadNetworkChanges UploadWithoutTheRemoval()
    {
        var geometry = UploadedRoad;

        var changes = RoadNetworkChanges.Start()
            .WithProvenance(new FakeProvenance())
            .Add(new AddRoadSegmentChange
            {
                RoadSegmentIdReference = new RoadSegmentIdReference(new RoadSegmentId(1), [new RoadSegmentTempId(993)]),
                Geometry = geometry.ToRoadSegmentGeometry(),
                GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Ingemeten,
                Status = RoadSegmentStatusV2.Gerealiseerd,
                AccessRestriction = Attribute<RoadSegmentAccessRestrictionV2>(geometry),
                Category = Attribute<RoadSegmentCategoryV2>(geometry),
                Morphology = Attribute<RoadSegmentMorphologyV2>(geometry),
                StreetNameId = Attribute<StreetNameLocalId>(geometry),
                MaintenanceAuthorityId = Attribute<OrganizationId>(geometry),
                SurfaceType = Attribute<RoadSegmentSurfaceTypeV2>(geometry),
                CarTrafficDirection = Attribute<RoadSegmentTrafficDirection>(geometry),
                BikeTrafficDirection = Attribute<RoadSegmentTrafficDirection>(geometry),
                PedestrianTrafficDirection = Attribute<RoadSegmentPedestrianTrafficDirection>(geometry)
            });

        foreach (var (roadNodeId, coordinate) in new[] { (StartNodeId, Start), (EndNodeId, End) })
        {
            changes = changes.Add(new ModifyRoadNodeChange
            {
                RoadNodeId = roadNodeId,
                Geometry = Node(coordinate),
                Grensknoop = false
            });
        }

        return changes;
    }

    private RoadSegment BuildCollectedRoadSegment(RoadSegmentId roadSegmentId, MultiLineString geometry, RoadNodeId startNodeId, RoadNodeId endNodeId)
    {
        return RoadSegment.Create(TestData.Segment1Added with
        {
            RoadSegmentId = roadSegmentId,
            Geometry = geometry.ToRoadSegmentGeometry(),
            Status = RoadSegmentStatusV2.Gerealiseerd,
            StartNodeId = startNodeId,
            EndNodeId = endNodeId,
            AccessRestriction = Attribute<RoadSegmentAccessRestrictionV2>(geometry),
            Category = Attribute<RoadSegmentCategoryV2>(geometry),
            Morphology = Attribute<RoadSegmentMorphologyV2>(geometry),
            StreetNameId = Attribute<StreetNameLocalId>(geometry),
            MaintenanceAuthorityId = Attribute<OrganizationId>(geometry),
            SurfaceType = Attribute<RoadSegmentSurfaceTypeV2>(geometry),
            CarTrafficDirection = Attribute<RoadSegmentTrafficDirection>(geometry),
            BikeTrafficDirection = Attribute<RoadSegmentTrafficDirection>(geometry),
            PedestrianTrafficDirection = Attribute<RoadSegmentPedestrianTrafficDirection>(geometry)
        }).WithoutChanges();
    }

    private RoadSegmentDynamicAttributeValues<T> Attribute<T>(MultiLineString geometry)
    {
        return new RoadSegmentDynamicAttributeValues<T>()
            .Add(Fixture.Create<T>(), RoadSegmentGeometry.Create(geometry));
    }

    private static MultiLineString Line(params Coordinate[] coordinates)
    {
        return new MultiLineString([new LineString(coordinates)])
        {
            SRID = WellknownSrids.Lambert08
        };
    }

    private static RoadNodeGeometry Node(Coordinate coordinate)
    {
        return new Point(coordinate) { SRID = WellknownSrids.Lambert08 }.ToRoadNodeGeometry();
    }

    private static InMemoryRoadNetworkIdGenerator IdGenerator()
    {
        return new InMemoryRoadNetworkIdGenerator(initialValue: 1000);
    }
}
