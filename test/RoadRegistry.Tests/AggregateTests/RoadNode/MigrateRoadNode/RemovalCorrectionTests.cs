namespace RoadRegistry.Tests.AggregateTests.RoadNode.MigrateRoadNode;

using AutoFixture;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.RoadNode.Events.V2;
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

// The other half of the neighbouring-municipality story (see NeighbourMergeTests, where the first upload merged a road
// across the border and took the road node with it). Here the municipality that uploaded first removed a road node on
// the border outright, along with the road segment of its own that ran to it. The neighbour was prepared against the
// extract from before that, and still brings that road node together with a road of its own - one lying entirely on
// its own side, which it is entitled to collect. The road node comes back, under the identifier it always had.
//
// One road runs north to south across the border between the two municipalities:
//
//   Inside (y=100)  --- deep inside the municipality that is uploading now
//     |
//   Border (y=0)    --- the road node the neighbour removed, just across the border
//     |
//   Across (y=-100) --- the far end of the road the neighbour removed along with it
//
// Each test sketches the three states it is about. In those sketches:
//
//   o  a road node                | a road, named ws-<identifier>
//   x  a road node that is removed
//   .  a road that is removed
public class RemovalCorrectionTests : RoadNetworkTestBase
{
    private static readonly Coordinate Inside = new(0, 100);
    private static readonly Coordinate Border = new(0, 0);
    private static readonly Coordinate Across = new(0, -100);

    private static readonly RoadNodeId InsideNodeId = new(1);
    private static readonly RoadNodeId BorderNodeId = new(2);
    private static readonly RoadNodeId AcrossNodeId = new(3);

    private static readonly RoadSegmentId NeighbourSegmentId = new(100);
    private static readonly RoadSegmentId UploadedSegmentId = new(200);

    // The case this is all about: the road node is brought back and keeps its identifier, and the road that runs to it
    // is collected as any other.
    //
    //   in the network            the upload                result
    //   o inside                  o inside                  o inside
    //   | ws-200                  | ws-200                  | ws-200
    //   x border                  o border                  o border
    //   . ws-100                                            . ws-100
    //   x across                                            x across
    [Fact]
    public void WhenTheNeighbourRemovedTheRoadNode_ThenItIsCollectedAgainUnderTheSameId()
    {
        var roadNetwork = BuildNetworkTheNeighbourLeftBehind();

        var result = roadNetwork.Migrate(Upload(), null, IdGenerator());

        result.Problems.Should().BeEmpty();

        var borderNode = roadNetwork.RoadNodes[BorderNodeId];
        borderNode.RoadNodeId.Should().Be(BorderNodeId);
        borderNode.IsRemoved.Should().BeFalse();
        borderNode.IsRemovedBecauseOfMigration.Should().BeFalse();
        borderNode.Geometry.Value.Coordinate.Should().Be(Border);
    }

    // The correction says no more than that the removal is undone; the road node is collected right after it, and that
    // is what carries the state the projections need to build their row again. The type follows from the topology:
    // one road runs to it, so it is an eindknoop.
    [Fact]
    public void WhenTheNeighbourRemovedTheRoadNode_ThenItIsCorrectedAndCollectedAgain()
    {
        var roadNetwork = BuildNetworkTheNeighbourLeftBehind();

        roadNetwork.Migrate(Upload(), null, IdGenerator());

        roadNetwork.RoadNodes[BorderNodeId].GetChanges().Should().SatisfyRespectively(
            first => first.Should().BeOfType<RoadNodeRemovalWasCorrectedBecauseOfMigration>()
                .Which.RoadNodeId.Should().Be(BorderNodeId),
            second => second.Should().BeOfType<RoadNodeWasMigrated>()
                .Which.Geometry.Value.Coordinate.Should().Be(Border),
            third => third.Should().BeOfType<RoadNodeTypeWasChanged>()
                .Which.Type.Should().Be(RoadNodeTypeV2.Eindknoop));
    }

    // Bringing the road node back is only worth anything if the road running to it finds it again. The spatial indexes
    // are built from what is not removed, so the road node has to be put back into them by hand.
    [Fact]
    public void WhenTheNeighbourRemovedTheRoadNode_ThenTheRoadRunningToItFindsItAgain()
    {
        var roadNetwork = BuildNetworkTheNeighbourLeftBehind();

        var result = roadNetwork.Migrate(Upload(), null, IdGenerator());

        result.Problems.Should().BeEmpty();

        var uploadedSegment = roadNetwork.RoadSegments[UploadedSegmentId];
        new[] { uploadedSegment.StartNodeId, uploadedSegment.EndNodeId }.Should().BeEquivalentTo([BorderNodeId, InsideNodeId]);
    }

    // The road node that was collected before it was removed is collected again from scratch rather than modified: the
    // correction drops its type for exactly that reason, so the projections that deleted their row get the geometry
    // and the grensknoop from a RoadNodeWasMigrated instead of the partial state a RoadNodeWasModified carries.
    [Fact]
    public void WhenTheRoadNodeHadAlreadyBeenCollected_ThenItIsCollectedAgainFromScratch()
    {
        var roadNetwork = BuildNetworkTheNeighbourLeftBehind(borderNodeHadBeenCollected: true);

        var result = roadNetwork.Migrate(Upload(), null, IdGenerator());

        result.Problems.Should().BeEmpty();
        roadNetwork.RoadNodes[BorderNodeId].GetChanges().Should().Contain(x => x is RoadNodeWasMigrated);
        roadNetwork.RoadNodes[BorderNodeId].GetChanges().Should().NotContain(x => x is RoadNodeWasModified);
    }

    // Which way round the uploaded road is drawn says nothing about which road nodes it runs to.
    //
    //   in the network            the upload                result
    //   o inside                  o inside                  o inside
    //   | ws-200                  | ws-200 (reversed)        | ws-200
    //   x border                  o border                  o border
    [Fact]
    public void WhenTheUploadedRoadLiesTheOtherWayAround_ThenTheRoadNodeIsStillCollectedAgain()
    {
        var roadNetwork = BuildNetworkTheNeighbourLeftBehind();

        var result = roadNetwork.Migrate(Upload(from: Inside, to: Border), null, IdGenerator());

        result.Problems.Should().BeEmpty();
        roadNetwork.RoadNodes[BorderNodeId].IsRemoved.Should().BeFalse();
    }

    // A road node on its own is not something the upload is entitled to bring back: nothing in it runs to the road
    // node, so there would be nothing holding it in the road network.
    //
    //   in the network            the upload                result
    //   o inside                  o inside                  o inside
    //   | ws-200                                             | ws-200
    //   x border                  o border                  x border  <- reported
    [Fact]
    public void WhenNoRoadInTheUploadRunsToTheRoadNode_ThenItIsStillReported()
    {
        var roadNetwork = BuildNetworkTheNeighbourLeftBehind();

        var result = roadNetwork.Migrate(UploadWithoutTheRoad(), null, IdGenerator());

        result.Problems.Select(x => x.Reason).Should().Contain("RoadNodeIsRemoved");
    }

    // The neighbour removed the road running to the road node as well. Bringing only the road node back would leave it
    // in the road network with nothing attached, so this stays an upload the municipality has to sort out itself.
    //
    //   in the network            the upload                result
    //   o inside                  o inside                  o inside
    //   . ws-200                  | ws-200                  . ws-200
    //   x border                  o border                  x border  <- reported
    [Fact]
    public void WhenTheRoadRunningToItWasRemovedAsWell_ThenItIsStillReported()
    {
        var roadNetwork = BuildNetworkTheNeighbourLeftBehind(uploadedRoadWasRemovedToo: true);

        var result = roadNetwork.Migrate(Upload(), null, IdGenerator());

        result.Problems.Select(x => x.Reason).Should().Contain("RoadNodeIsRemoved");
    }

    // A road node merged away rather than removed by an inwinning is removed for good: that is the merge's own doing
    // and not a road node a neighbouring municipality took from under this upload. NeighbourMergeTests covers what
    // becomes of such an upload; here it is simply reported.
    //
    //   in the network            the upload                result
    //   o inside                  o inside                  o inside
    //   | ws-200                  | ws-200                  | ws-200
    //   x border                  o border                  x border  <- reported
    [Fact]
    public void WhenTheRoadNodeWasMergedAwayRatherThanRemovedByAnInwinning_ThenItIsStillReported()
    {
        var roadNetwork = BuildNetworkTheNeighbourLeftBehind(borderNodeWasMergedAway: true);

        var result = roadNetwork.Migrate(Upload(), null, IdGenerator());

        result.Problems.Select(x => x.Reason).Should().Contain("RoadNodeIsRemoved");
    }

    // A road node the road network never knew is not a removal to correct, however well the upload lines up with it.
    [Fact]
    public void WhenTheRoadNodeWasNeverThere_ThenItIsReportedAsNotFound()
    {
        var roadNetwork = BuildNetwork(
            roadNodes: [(InsideNodeId, Inside)],
            roadSegments: [(UploadedSegmentId, Border, Inside, BorderNodeId, InsideNodeId)]);

        var result = roadNetwork.Migrate(Upload(), null, IdGenerator());

        result.Problems.Select(x => x.Reason).Should().Contain("RoadNodeNotFound");
    }

    // Nothing to correct: the road node the upload names is right where it left it.
    [Fact]
    public void WhenTheRoadNodeIsStillThere_ThenNothingIsCorrected()
    {
        var roadNetwork = BuildNetwork(
            roadNodes: [(InsideNodeId, Inside), (BorderNodeId, Border)],
            roadSegments: [(UploadedSegmentId, Border, Inside, BorderNodeId, InsideNodeId)]);

        var result = roadNetwork.Migrate(Upload(), null, IdGenerator());

        result.Problems.Should().BeEmpty();
        roadNetwork.RoadNodes[BorderNodeId].GetChanges().Should()
            .NotContain(x => x is RoadNodeRemovalWasCorrectedBecauseOfMigration);
    }

    // The network as the second municipality finds it: its own road to the border, the road node the neighbour removed,
    // and the road the neighbour removed along with it.
    private ScopedRoadNetwork BuildNetworkTheNeighbourLeftBehind(
        bool borderNodeHadBeenCollected = false,
        bool borderNodeWasMergedAway = false,
        bool uploadedRoadWasRemovedToo = false)
    {
        return BuildNetwork(
            roadNodes: [(InsideNodeId, Inside)],
            roadSegments: uploadedRoadWasRemovedToo
                ? []
                : [(UploadedSegmentId, Border, Inside, BorderNodeId, InsideNodeId)],
            removedRoadNodes: [(BorderNodeId, Border), (AcrossNodeId, Across)],
            removedRoadSegments: uploadedRoadWasRemovedToo
                ? [(NeighbourSegmentId, Across, Border, AcrossNodeId, BorderNodeId), (UploadedSegmentId, Border, Inside, BorderNodeId, InsideNodeId)]
                : [(NeighbourSegmentId, Across, Border, AcrossNodeId, BorderNodeId)],
            borderNodeHadBeenCollected: borderNodeHadBeenCollected,
            borderNodeWasMergedAway: borderNodeWasMergedAway);
    }

    // What the municipality uploads: the road node the neighbour removed, the one it did not, and its own road running
    // between the two coordinates given.
    private RoadNetworkChanges Upload(Coordinate? from = null, Coordinate? to = null)
    {
        var geometry = Line(from ?? Border, to ?? Inside);

        return UploadWithoutTheRoad()
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
    }

    private static RoadNetworkChanges UploadWithoutTheRoad()
    {
        return RoadNetworkChanges.Start()
            .WithProvenance(new FakeProvenance())
            .Add(new ModifyRoadNodeChange
            {
                RoadNodeId = BorderNodeId,
                Geometry = Node(Border),
                Grensknoop = false
            })
            .Add(new ModifyRoadNodeChange
            {
                RoadNodeId = InsideNodeId,
                Geometry = Node(Inside),
                Grensknoop = false
            });
    }

    private ScopedRoadNetwork BuildNetwork(
        (RoadNodeId Id, Coordinate Coordinate)[] roadNodes,
        (RoadSegmentId Id, Coordinate From, Coordinate To, RoadNodeId StartNodeId, RoadNodeId EndNodeId)[] roadSegments,
        (RoadNodeId Id, Coordinate Coordinate)[]? removedRoadNodes = null,
        (RoadSegmentId Id, Coordinate From, Coordinate To, RoadNodeId StartNodeId, RoadNodeId EndNodeId)[]? removedRoadSegments = null,
        bool borderNodeHadBeenCollected = false,
        bool borderNodeWasMergedAway = false)
    {
        return new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(),
            roadNodes: roadNodes
                .Select(x => RoadNode.CreateForMigration(x.Id, Node(x.Coordinate)))
                .Concat((removedRoadNodes ?? []).Select(x =>
                    BuildRemovedRoadNode(x, borderNodeHadBeenCollected, borderNodeWasMergedAway)))
                .ToArray(),
            roadSegments: roadSegments
                .Select(BuildRoadSegment)
                .Concat((removedRoadSegments ?? []).Select(BuildRemovedRoadSegment))
                .ToArray());
    }

    private static RoadSegment BuildRoadSegment(
        (RoadSegmentId Id, Coordinate From, Coordinate To, RoadNodeId StartNodeId, RoadNodeId EndNodeId) roadSegment)
    {
        return RoadSegment.CreateForMigration(
            roadSegment.Id,
            Line(roadSegment.From, roadSegment.To).ToRoadSegmentGeometry(),
            RoadSegmentStatusV2.Gerealiseerd,
            roadSegment.StartNodeId,
            roadSegment.EndNodeId);
    }

    private static RoadSegment BuildRemovedRoadSegment(
        (RoadSegmentId Id, Coordinate From, Coordinate To, RoadNodeId StartNodeId, RoadNodeId EndNodeId) roadSegment)
    {
        var removed = BuildRoadSegment(roadSegment);
        removed.RemoveBecauseOfMigration(new FakeProvenance());

        return removed.WithoutChanges();
    }

    // The road network is loaded with the road nodes that were removed from it still in place, so that is how a road
    // node a neighbouring inwinning removed arrives here. Which of the two removals it was decides whether it may be
    // corrected: only an inwinning's.
    private static RoadNode BuildRemovedRoadNode(
        (RoadNodeId Id, Coordinate Coordinate) roadNode,
        bool hadBeenCollected,
        bool wasMergedAway)
    {
        var removed = hadBeenCollected
            ? RoadNode.Create(new RoadNodeWasAdded
            {
                RoadNodeId = roadNode.Id,
                Geometry = Node(roadNode.Coordinate),
                Grensknoop = false,
                Type = RoadNodeTypeV2.Eindknoop,
                Provenance = new ProvenanceData(new FakeProvenance())
            })
            : RoadNode.CreateForMigration(roadNode.Id, Node(roadNode.Coordinate));

        if (wasMergedAway)
        {
            removed.Remove(new FakeProvenance());
        }
        else
        {
            removed.RemoveBecauseOfMigration(new FakeProvenance());
        }

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
