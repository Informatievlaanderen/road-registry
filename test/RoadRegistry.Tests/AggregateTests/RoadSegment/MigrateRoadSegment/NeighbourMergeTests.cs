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
// Each test sketches the three states it is about. In those sketches:
//
//   o  a road node                | a road
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

    // The neighbour merged North-Middle with Middle-Border and kept the lot, so Middle is gone. This upload still
    // brings Middle-South, of which Middle-Border has already been collected.
    //
    //   in the network            the upload                result
    //   o north                                             o north
    //   | 100                                               | 100
    //   x middle                  x middle                  x middle
    //   |                         : taken over              |
    //   o border                  o border                  o border
    //   | 200                     | 200                     | 200
    //   o south                   o south                   o south
    [Fact]
    public void WhenTheNeighbourTookPartOfTheRoad_ThenOnlyWhatIsLeftIsCollected()
    {
        var roadNetwork = BuildNetwork(
            roadNodes: [(NorthNodeId, North), (BorderNodeId, Border), (SouthNodeId, South)],
            roadSegments:
            [
                (NeighbourSegmentId, North, Border, NorthNodeId, BorderNodeId),
                (UploadedSegmentId, Border, South, BorderNodeId, SouthNodeId)
            ],
            removedRoadNodes: [(MiddleNodeId, Middle)]);

        var result = roadNetwork.Migrate(Upload(Middle, South, (BorderNodeId, Border), (SouthNodeId, South)), null, IdGenerator());

        result.Problems.Should().BeEmpty();

        var collected = roadNetwork.RoadSegments[UploadedSegmentId];
        collected.Geometry.Value.Length.Should().BeApproximately(50, 0.01);
        collected.Geometry.Value.Coordinates.Should().Contain(c => c.Equals2D(Border));
        collected.Geometry.Value.Coordinates.Should().NotContain(c => c.Equals2D(Middle));
        collected.StartNodeId.Should().Be(BorderNodeId);
        collected.EndNodeId.Should().Be(SouthNodeId);
    }

    // The same upload as above, read for what it does to the attributes. They are positioned along the road as it was
    // uploaded, so the stretch that was cut off has to be taken off them too - otherwise they run past the end of
    // what is left.
    //
    //   the upload                result
    //   x middle                  x middle
    //   : taken over
    //   o border                  o border
    //   | 200                     | 200
    //   o south                   o south
    //
    //   morphology 0..100         morphology 0..50
    [Fact]
    public void WhenTheNeighbourTookPartOfTheRoad_ThenTheAttributesAreCutBackWithIt()
    {
        var roadNetwork = BuildNetwork(
            roadNodes: [(NorthNodeId, North), (BorderNodeId, Border), (SouthNodeId, South)],
            roadSegments:
            [
                (NeighbourSegmentId, North, Border, NorthNodeId, BorderNodeId),
                (UploadedSegmentId, Border, South, BorderNodeId, SouthNodeId)
            ],
            removedRoadNodes: [(MiddleNodeId, Middle)]);

        roadNetwork.Migrate(Upload(Middle, South, (BorderNodeId, Border), (SouthNodeId, South)), null, IdGenerator())
            .Problems.Should().BeEmpty();

        var attributes = roadNetwork.RoadSegments[UploadedSegmentId].Attributes!;
        attributes.Morphology.Values.Should().ContainSingle()
            .Which.Coverage.To.ToDouble().Should().BeApproximately(50, 0.01);
    }

    // The neighbour merged all the way down to Border, so the road this upload brings was taken whole. It is not in
    // the road network any more either, which is exactly what used to be reported as a missing road segment.
    //
    //   in the network            the upload                result
    //   o north                                             o north
    //   | 100                                               | 100
    //   x middle                  x middle                  x middle
    //   |                         : taken over              |
    //   o border                  o border                  o border
    //
    // Nothing of 200 is left to collect, and that is not an error.
    [Fact]
    public void WhenTheNeighbourTookTheWholeRoad_ThenNothingIsCollectedForIt()
    {
        var roadNetwork = BuildNetwork(
            roadNodes: [(NorthNodeId, North), (BorderNodeId, Border)],
            roadSegments: [(NeighbourSegmentId, North, Border, NorthNodeId, BorderNodeId)],
            removedRoadNodes: [(MiddleNodeId, Middle)]);

        var result = roadNetwork.Migrate(Upload(Middle, Border, (BorderNodeId, Border)), null, IdGenerator());

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
    //                             | 200
    //   o border                  o border                  o border
    //   | 200                     | 200                     | 200
    //   o south                   o south                   o south
    [Fact]
    public void WhenTheRoadNodeIsGoneWithNoRoadTakingItsPlace_ThenItIsStillReported()
    {
        var roadNetwork = BuildNetwork(
            roadNodes: [(BorderNodeId, Border), (SouthNodeId, South)],
            roadSegments: [(UploadedSegmentId, Border, South, BorderNodeId, SouthNodeId)],
            removedRoadNodes: [(MiddleNodeId, Middle)]);

        var result = roadNetwork.Migrate(Upload(Middle, South, (BorderNodeId, Border), (SouthNodeId, South)), null, IdGenerator());

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
    //   | 200                     | 200                     | 200
    //   o south                   o south                   o south
    [Fact]
    public void WhenTheRoadNodeWasNeverThere_ThenItIsReportedAsNotFound()
    {
        // Segment 100 lies exactly where the uploaded road leaves Middle, so a merge would have been accounted for -
        // but the road node was never part of this road network to begin with.
        var roadNetwork = BuildNetwork(
            roadNodes: [(NorthNodeId, North), (BorderNodeId, Border), (SouthNodeId, South)],
            roadSegments:
            [
                (NeighbourSegmentId, North, Border, NorthNodeId, BorderNodeId),
                (UploadedSegmentId, Border, South, BorderNodeId, SouthNodeId)
            ]);

        var result = roadNetwork.Migrate(Upload(Middle, South, (BorderNodeId, Border), (SouthNodeId, South)), null, IdGenerator());

        result.Problems.Select(x => x.Reason).Should().Contain("RoadNodeNotFound");
    }

    // Nothing to account for: the road node the upload names is right where it left it.
    //
    //   in the network            the upload                result
    //   o middle                  o middle                  o middle
    //   | 200                     | 200                     | 200
    //   o south                   o south                   o south
    [Fact]
    public void WhenTheRoadNodeIsStillThere_ThenTheRoadIsCollectedWhole()
    {
        var roadNetwork = BuildNetwork(
            roadNodes: [(MiddleNodeId, Middle), (SouthNodeId, South)],
            roadSegments: [(UploadedSegmentId, Middle, South, MiddleNodeId, SouthNodeId)]);

        var result = roadNetwork.Migrate(Upload(Middle, South, (SouthNodeId, South)), null, IdGenerator());

        result.Problems.Should().BeEmpty();
        roadNetwork.RoadSegments[UploadedSegmentId].Geometry.Value.Length.Should().BeApproximately(100, 0.01);
    }

    // What the municipality uploads: the road node that is gone, the ones that are not, and the road running between
    // the two coordinates given.
    private RoadNetworkChanges Upload(Coordinate from, Coordinate to, params (RoadNodeId Id, Coordinate Coordinate)[] otherRoadNodes)
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

        foreach (var (roadNodeId, coordinate) in otherRoadNodes)
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

    private ScopedRoadNetwork BuildNetwork(
        (RoadNodeId Id, Coordinate Coordinate)[] roadNodes,
        (RoadSegmentId Id, Coordinate From, Coordinate To, RoadNodeId StartNodeId, RoadNodeId EndNodeId)[] roadSegments,
        (RoadNodeId Id, Coordinate Coordinate)[]? removedRoadNodes = null)
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
                .ToArray());
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
