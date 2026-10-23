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

// Two neighbouring municipalities are collected independently, and the upload that lands first may merge a road that
// runs across their border. That merge removes the road node the two roads met at, while the second upload - prepared
// against the extract from before it - still names that road node and still carries the stretch that was taken.
//
// One road runs north to south through both municipalities:
//
//   North (y=200)  --- the far end of what the neighbour collected
//     |
//   Middle (y=100) --- the road node the neighbour's merge removed
//     |
//   Border (y=50)  --- the road node both uploads still share
//     |
//   South (y=0)    --- the far end of what this upload collects
//
// Cutting the upload back is only half of it. What is left then runs up to Border, where the road the neighbour
// collected already ends - so unless Border is a grensknoop, the road network merges the two and does away with it.
// That is not something the cut arranges: it is what every inwinning does with a road node it no longer needs.
//
// Each test sketches the three states it is about. In those sketches:
//
//   o  a road node                | a road, named ws-<identifier>
//   x  the road node an earlier inwinning merged away
//   :  the stretch that inwinning already took
public class NeighbourMergeTests : RoadNetworkTestBase
{
    private static readonly Coordinate North = new(0, 200);
    private static readonly Coordinate Middle = new(0, 100);
    private static readonly Coordinate Border = new(0, 50);
    private static readonly Coordinate South = new(0, 0);

    private static readonly RoadNodeId NorthNodeId = new(1);
    private static readonly RoadNodeId MiddleNodeId = new(2);
    private static readonly RoadNodeId BorderNodeId = new(3);
    private static readonly RoadNodeId SouthNodeId = new(4);

    private static readonly RoadSegmentId NeighbourSegmentId = new(100);
    private static readonly RoadSegmentId UploadedSegmentId = new(200);

    // The upload says Border is a grensknoop, so the two roads stay apart and what the cut left behind is plain to see.
    //
    //   in the network            the upload                result
    //   o north                                             o north
    //   | ws-100                                            | ws-100
    //   x middle                  x middle                  x middle
    //   |                         : taken over              |
    //   o border                  o border                  o border  (grensknoop)
    //   | ws-200                  | ws-200                  | ws-200
    //   o south                   o south                   o south
    [Fact]
    public void WhenTheSharedRoadNodeIsAGrensknoop_ThenOnlyWhatIsLeftIsCollected()
    {
        var roadNetwork = BuildNetworkWithCollectedRoad(North, Border);

        var result = roadNetwork.Migrate(Upload(Middle, South, BorderIsGrensknoop), null, IdGenerator());

        result.Problems.Should().BeEmpty();

        var collected = roadNetwork.RoadSegments[UploadedSegmentId];
        collected.Geometry.Value.Length.Should().BeApproximately(50, 0.01);
        collected.Geometry.Value.Coordinates.Should().Contain(c => c.Equals2D(Border));
        collected.Geometry.Value.Coordinates.Should().NotContain(c => c.Equals2D(Middle));
        collected.StartNodeId.Should().Be(BorderNodeId);
        collected.EndNodeId.Should().Be(SouthNodeId);
    }

    // The same upload and the same grensknoop, read for what it does to the attributes. They are positioned along the
    // road as it was uploaded, so the stretch that was cut off has to be taken off them too - otherwise they run past
    // the end of what is left.
    //
    //   the upload                result
    //   x middle                  x middle
    //   : taken over
    //   o border                  o border
    //   | ws-200                  | ws-200
    //   o south                   o south
    //
    //   morphology 0..100         morphology 0..50
    [Fact]
    public void WhenTheSharedRoadNodeIsAGrensknoop_ThenTheAttributesAreCutBackWithIt()
    {
        var roadNetwork = BuildNetworkWithCollectedRoad(North, Border);

        roadNetwork.Migrate(Upload(Middle, South, BorderIsGrensknoop), null, IdGenerator())
            .Problems.Should().BeEmpty();

        var attributes = roadNetwork.RoadSegments[UploadedSegmentId].Attributes!;
        attributes.Morphology.Values.Should().ContainSingle()
            .Which.Coverage.To.ToDouble().Should().BeApproximately(50, 0.01);
    }

    // Border is an ordinary road node, so once the upload is cut back to it there is nothing left for it to hold
    // apart: the two roads are merged into one and the road node goes with it.
    //
    //   in the network            the upload                result
    //   o north                                             o north
    //   | ws-100                                            |
    //   x middle                  x middle                  | ws-100
    //   |                         : taken over              |
    //   o border                  o border                  |
    //   | ws-200                  | ws-200                  |
    //   o south                   o south                   o south
    //
    // The road that is left is ws-100: of the two it is the longer, so it is the one that keeps its identifier, and
    // ws-200 is historized behind it. It runs from North to South now, not just the stretch ws-100 used to cover.
    [Fact]
    public void WhenTheSharedRoadNodeIsNotNeeded_ThenWhatIsLeftIsMergedWithIt()
    {
        var roadNetwork = BuildNetworkWithCollectedRoad(North, Border);

        var result = roadNetwork.Migrate(Upload(Middle, South, BorderIsOrdinary), null, IdGenerator());

        result.Problems.Should().BeEmpty();
        ThenTheRoadIsOneWholeAgain(roadNetwork);
    }

    // Which end a road was drawn from says nothing about where it lies, so the cut and the merge have to come out the
    // same whichever way around the two roads were digitised.
    [Fact]
    public void WhenTheCollectedRoadLiesTheOtherWayAround_ThenWhatIsLeftIsStillMergedWithIt()
    {
        var roadNetwork = BuildNetworkWithCollectedRoad(Border, North);

        var result = roadNetwork.Migrate(Upload(Middle, South, BorderIsOrdinary), null, IdGenerator());

        result.Problems.Should().BeEmpty();
        ThenTheRoadIsOneWholeAgain(roadNetwork);
    }

    // The uploaded road runs south to middle, so the stretch to cut off sits at its end rather than at its start.
    [Fact]
    public void WhenTheUploadedRoadLiesTheOtherWayAround_ThenWhatIsLeftIsStillMergedWithIt()
    {
        var roadNetwork = BuildNetworkWithCollectedRoad(North, Border);

        var result = roadNetwork.Migrate(Upload(South, Middle, BorderIsOrdinary), null, IdGenerator());

        result.Problems.Should().BeEmpty();
        ThenTheRoadIsOneWholeAgain(roadNetwork);
    }

    [Fact]
    public void WhenBothRoadsLieTheOtherWayAround_ThenWhatIsLeftIsStillMergedWithIt()
    {
        var roadNetwork = BuildNetworkWithCollectedRoad(Border, North);

        var result = roadNetwork.Migrate(Upload(South, Middle, BorderIsOrdinary), null, IdGenerator());

        result.Problems.Should().BeEmpty();
        ThenTheRoadIsOneWholeAgain(roadNetwork);
    }

    // The uploaded road the other way around, with Border held apart by a grensknoop, so what the cut left behind can
    // be read off directly - at the end of the geometry this time, and the attributes with it.
    [Fact]
    public void WhenTheUploadedRoadLiesTheOtherWayAround_ThenOnlyWhatIsLeftIsCollected()
    {
        var roadNetwork = BuildNetworkWithCollectedRoad(North, Border);

        var result = roadNetwork.Migrate(Upload(South, Middle, BorderIsGrensknoop), null, IdGenerator());

        result.Problems.Should().BeEmpty();

        var collected = roadNetwork.RoadSegments[UploadedSegmentId];
        collected.Geometry.Value.Length.Should().BeApproximately(50, 0.01);
        collected.Geometry.Value.Coordinates.Should().NotContain(c => c.Equals2D(Middle));
        collected.StartNodeId.Should().Be(SouthNodeId);
        collected.EndNodeId.Should().Be(BorderNodeId);
        collected.Attributes!.Morphology.Values.Should().ContainSingle()
            .Which.Coverage.To.ToDouble().Should().BeApproximately(50, 0.01);
    }

    // The neighbour merged all the way down to Border, so the road this upload brings was taken whole. It is not in
    // the road network any more either, which is exactly what used to be reported as a missing road segment.
    //
    //   in the network            the upload                result
    //   o north                                             o north
    //   | ws-100                                            | ws-100
    //   x middle                  x middle                  x middle
    //   |                         : taken over              |
    //   o border                  o border                  o border
    //
    // Nothing of ws-200 is left to collect, and that is not an error.
    [Fact]
    public void WhenTheNeighbourTookTheWholeRoad_ThenNothingIsCollectedForIt()
    {
        var roadNetwork = BuildNetwork(
            roadNodes: [(NorthNodeId, North), (BorderNodeId, Border)],
            roadSegments: [],
            removedRoadNodes: [(MiddleNodeId, Middle)],
            collectedRoadSegments: [(NeighbourSegmentId, North, Border, NorthNodeId, BorderNodeId)]);

        var result = roadNetwork.Migrate(
            Upload(Middle, Border, [(BorderNodeId, Border, true)]), null, IdGenerator());

        result.Problems.Should().BeEmpty();
        result.Summary.RoadSegments.Added.Should().BeEmpty();
        result.Summary.RoadSegments.Modified.Should().NotContain(UploadedSegmentId);
        roadNetwork.RoadSegments.Should().NotContainKey(UploadedSegmentId);
    }

    // The road node was removed all right, but nothing lies where the uploaded road leaves it, so there is no earlier
    // merge to account for it. It is reported as removed, which is as much as this upload can be told.
    //
    //   in the network            the upload                result
    //   x middle                  x middle                  x middle
    //                             | ws-200
    //   o border                  o border                  o border
    //   | ws-200                  | ws-200                  | ws-200
    //   o south                   o south                   o south
    [Fact]
    public void WhenTheRoadNodeIsGoneWithNoRoadTakingItsPlace_ThenItIsStillReported()
    {
        var roadNetwork = BuildNetwork(
            roadNodes: [(BorderNodeId, Border), (SouthNodeId, South)],
            roadSegments: [(UploadedSegmentId, Border, South, BorderNodeId, SouthNodeId)],
            removedRoadNodes: [(MiddleNodeId, Middle)]);

        var result = roadNetwork.Migrate(Upload(Middle, South, BorderIsOrdinary), null, IdGenerator());

        result.Problems.Select(x => x.Reason).Should().Contain("RoadNodeIsRemoved");
    }

    // A road node the road network never knew is not an earlier merge to account for, whatever lies where the upload
    // leaves it. Telling the two apart is the whole point of looking at the road node that is removed rather than at
    // the one that cannot be found.
    //
    //   in the network            the upload                result
    //                             x middle
    //                             : would fit
    //   o border                  o border                  o border
    //   | ws-200                  | ws-200                  | ws-200
    //   o south                   o south                   o south
    [Fact]
    public void WhenTheRoadNodeWasNeverThere_ThenItIsReportedAsNotFound()
    {
        // The collected road lies exactly where the uploaded one leaves Middle, so a merge would have been accounted
        // for - but the road node was never part of this road network to begin with.
        var roadNetwork = BuildNetwork(
            roadNodes: [(NorthNodeId, North), (BorderNodeId, Border), (SouthNodeId, South)],
            roadSegments: [(UploadedSegmentId, Border, South, BorderNodeId, SouthNodeId)],
            collectedRoadSegments: [(NeighbourSegmentId, North, Border, NorthNodeId, BorderNodeId)]);

        var result = roadNetwork.Migrate(Upload(Middle, South, BorderIsOrdinary), null, IdGenerator());

        result.Problems.Select(x => x.Reason).Should().Contain("RoadNodeNotFound");
    }

    // Nothing to account for: the road node the upload names is right where it left it.
    //
    //   in the network            the upload                result
    //   o middle                  o middle                  o middle
    //   | ws-200                  | ws-200                  | ws-200
    //   o south                   o south                   o south
    [Fact]
    public void WhenTheRoadNodeIsStillThere_ThenTheRoadIsCollectedWhole()
    {
        var roadNetwork = BuildNetwork(
            roadNodes: [(MiddleNodeId, Middle), (SouthNodeId, South)],
            roadSegments: [(UploadedSegmentId, Middle, South, MiddleNodeId, SouthNodeId)]);

        var result = roadNetwork.Migrate(
            Upload(Middle, South, [(SouthNodeId, South, false)]), null, IdGenerator());

        result.Problems.Should().BeEmpty();
        roadNetwork.RoadSegments[UploadedSegmentId].Geometry.Value.Length.Should().BeApproximately(100, 0.01);
    }

    // One road from North to South, and nothing left of the road node that used to sit between the two it was made of.
    private static void ThenTheRoadIsOneWholeAgain(ScopedRoadNetwork roadNetwork)
    {
        var merged = roadNetwork.RoadSegments.Values
            .Where(x => !x.IsRemoved && x.Status == RoadSegmentStatusV2.Gerealiseerd)
            .Should().ContainSingle().Subject;

        // The longer of the two keeps its identifier, so what is left is the road the neighbour collected - grown to
        // the whole of it.
        merged.RoadSegmentId.Should().Be(NeighbourSegmentId);
        merged.Geometry.Value.Length.Should().BeApproximately(200, 0.01);

        roadNetwork.RoadSegments[UploadedSegmentId].Status.Should().Be(RoadSegmentStatusV2.Gehistoreerd);
        roadNetwork.RoadNodes[BorderNodeId].IsRemoved.Should().BeTrue();
    }

    // What the upload says about the road nodes it did not lose. Whether Border is a grensknoop is the upload's to
    // state, and it decides whether the road network still needs it once the cut is made.
    private static readonly (RoadNodeId Id, Coordinate Coordinate, bool Grensknoop)[] BorderIsGrensknoop =
        [(BorderNodeId, Border, true), (SouthNodeId, South, false)];

    private static readonly (RoadNodeId Id, Coordinate Coordinate, bool Grensknoop)[] BorderIsOrdinary =
        [(BorderNodeId, Border, false), (SouthNodeId, South, false)];

    // The network as the second municipality finds it: the road the neighbour collected, the road this upload is
    // about, and the road node their merge removed.
    private ScopedRoadNetwork BuildNetworkWithCollectedRoad(Coordinate collectedFrom, Coordinate collectedTo)
    {
        var fromNorth = collectedFrom.Equals2D(North);

        return BuildNetwork(
            roadNodes: [(NorthNodeId, North), (BorderNodeId, Border), (SouthNodeId, South)],
            roadSegments: [(UploadedSegmentId, Border, South, BorderNodeId, SouthNodeId)],
            removedRoadNodes: [(MiddleNodeId, Middle)],
            collectedRoadSegments:
            [
                (NeighbourSegmentId, collectedFrom, collectedTo,
                    fromNorth ? NorthNodeId : BorderNodeId,
                    fromNorth ? BorderNodeId : NorthNodeId)
            ]);
    }

    // What the municipality uploads: the road node that is gone, the ones that are not, and the road running between
    // the two coordinates given.
    private RoadNetworkChanges Upload(Coordinate from, Coordinate to, (RoadNodeId Id, Coordinate Coordinate, bool Grensknoop)[] otherRoadNodes)
    {
        var geometry = Line(from, to);

        var changes = RoadNetworkChanges.Start()
            .WithProvenance(new FakeProvenance())
            .Add(new ModifyRoadNodeChange
            {
                RoadNodeId = MiddleNodeId,
                Geometry = Node(Middle),
                Grensknoop = false
            })
            .Add(new ModifyRoadSegmentChange
            {
                RoadSegmentIdReference = new RoadSegmentIdReference(UploadedSegmentId),
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

        foreach (var (roadNodeId, coordinate, grensknoop) in otherRoadNodes)
        {
            changes = changes.Add(new ModifyRoadNodeChange
            {
                RoadNodeId = roadNodeId,
                Geometry = Node(coordinate),
                Grensknoop = grensknoop
            });
        }

        return changes;
    }

    private ScopedRoadNetwork BuildNetwork(
        (RoadNodeId Id, Coordinate Coordinate)[] roadNodes,
        (RoadSegmentId Id, Coordinate From, Coordinate To, RoadNodeId StartNodeId, RoadNodeId EndNodeId)[] roadSegments,
        (RoadNodeId Id, Coordinate Coordinate)[]? removedRoadNodes = null,
        (RoadSegmentId Id, Coordinate From, Coordinate To, RoadNodeId StartNodeId, RoadNodeId EndNodeId)[]? collectedRoadSegments = null)
    {
        return new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(),
            roadNodes: roadNodes
                .Select(x => RoadNode.CreateForMigration(x.Id, Node(x.Coordinate)))
                .Concat((removedRoadNodes ?? []).Select(BuildRemovedRoadNode))
                .ToArray(),
            roadSegments: roadSegments
                .Select(x => RoadSegment.CreateForMigration(
                    x.Id,
                    Line(x.From, x.To).ToRoadSegmentGeometry(),
                    RoadSegmentStatusV2.Gerealiseerd,
                    x.StartNodeId,
                    x.EndNodeId))
                .Concat((collectedRoadSegments ?? []).Select(BuildCollectedRoadSegment))
                .ToArray());
    }

    // What the neighbour already collected: a road segment with V2 attributes. Without them the road network treats
    // the road node between two roads as one it still needs, so nothing would ever be merged.
    private RoadSegment BuildCollectedRoadSegment(
        (RoadSegmentId Id, Coordinate From, Coordinate To, RoadNodeId StartNodeId, RoadNodeId EndNodeId) roadSegment)
    {
        var geometry = Line(roadSegment.From, roadSegment.To);

        return RoadSegment.Create(TestData.Segment1Added with
        {
            RoadSegmentId = roadSegment.Id,
            Geometry = geometry.ToRoadSegmentGeometry(),
            Status = RoadSegmentStatusV2.Gerealiseerd,
            StartNodeId = roadSegment.StartNodeId,
            EndNodeId = roadSegment.EndNodeId,
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

    // The road network is loaded with the road nodes that were removed from it still in place, so that is how a road
    // node an earlier inwinning merged away arrives here.
    private static RoadNode BuildRemovedRoadNode((RoadNodeId Id, Coordinate Coordinate) roadNode)
    {
        var removed = RoadNode.CreateForMigration(roadNode.Id, Node(roadNode.Coordinate));
        removed.Remove(new FakeProvenance());

        return removed.WithoutChanges();
    }

    private RoadSegmentDynamicAttributeValues<T> Attribute<T>(MultiLineString geometry)
    {
        return new RoadSegmentDynamicAttributeValues<T>()
            .Add(Fixture.Create<T>(), RoadSegmentGeometry.Create(geometry));
    }

    private static MultiLineString Line(Coordinate from, Coordinate to)
    {
        return new MultiLineString([new LineString([from, to])])
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
