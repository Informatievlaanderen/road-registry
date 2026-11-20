namespace RoadRegistry.ScopedRoadNetwork;

using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.ValueObjects;

public partial class ScopedRoadNetwork
{
    // The other half of the neighbouring-municipality story (see ScopedRoadNetwork-Migrate-NeighbourMerge.cs). There,
    // the municipality that uploaded first had merged a road across the border and taken the road node with it. Here it
    // removed them outright - the road node, the road segment that ran to it, or both - and the neighbour still brings
    // them, because they lie on its own side as well and it is entitled to collect them.
    //
    // Refusing that would leave the second municipality unable to collect what it owns because of what its neighbour
    // did on the border. So they come back instead, under the identifiers they always had: it is the same road node and
    // the same road segment, and everything that refers to them by number has to keep pointing at them.

    // What this upload brings back, worked out before a single change is applied.
    private sealed record RemovalCorrections(
        IReadOnlyCollection<RoadNodeId> RoadNodeIds,
        IReadOnlyCollection<RoadSegmentId> RoadSegmentIds)
    {
        public static readonly RemovalCorrections None = new([], []);
    }

    // A road this upload brings, weighed only by where it runs. KnownToTheRegister tells the two apart for the log:
    // a road the register already has is worth naming by its number, while a road that is added still has to be given
    // one, and the number on its reference is not the number it will end up with.
    private sealed record CollectedRoad(RoadSegmentIdReference IdReference, MultiLineString Geometry, bool KnownToTheRegister)
    {
        public string Describe()
        {
            if (KnownToTheRegister)
            {
                return $"road segment {IdReference.RoadSegmentId.ToInt32()}";
            }

            return IdReference.TempIds is { Count: > 0 }
                ? $"the road this upload brings as temporary road segment(s) {IdReference.GetTempIdsAsString()}"
                : "a road this upload draws anew";
        }
    }

    // Run after ResolveNeighbourMerges and given its answer: what that merge accounts for is not this, and the two
    // never claim the same thing anyway - a merge removes what it no longer needs with RoadNodeWasRemoved, which is
    // not a removal any inwinning may undo.
    //
    // The road segments are worked out first: one that is brought back is there to carry a road node again, so it
    // counts when the road nodes are weighed right after.
    private RemovalCorrections ResolveRemovalCorrections(
        RoadNetworkChanges changes,
        NeighbourMergeResolutions neighbourMerges,
        ScopedRoadNetworkChangeContext context)
    {
        var correctedRoadSegmentIds = ResolveRoadSegmentRemovalCorrections(changes, neighbourMerges, context);
        var correctedRoadNodeIds = ResolveRoadNodeRemovalCorrections(changes, neighbourMerges, correctedRoadSegmentIds, context);

        if (correctedRoadNodeIds.Count == 0 && correctedRoadSegmentIds.Count == 0)
        {
            return RemovalCorrections.None;
        }

        return new RemovalCorrections(correctedRoadNodeIds, correctedRoadSegmentIds);
    }

    // A road segment a neighbouring inwinning removed that this upload names again. Unlike a road node, it needs
    // nothing else to justify it: a road segment the uploader collected is a road it owns, and it carries its own
    // geometry and attributes. What it does need is to be collectable from scratch, because that is what happens to
    // it once it is back.
    private IReadOnlyCollection<RoadSegmentId> ResolveRoadSegmentRemovalCorrections(
        RoadNetworkChanges changes,
        NeighbourMergeResolutions neighbourMerges,
        ScopedRoadNetworkChangeContext context)
    {
        var corrected = changes
            .OfType<ModifyRoadSegmentChange>()
            // Everything a road segment needs to be collected again has to be on the change: it is migrated from
            // scratch once it is back, and a migration is only one when it carries the lot.
            .Where(change => CanBeCollectedFromScratch(change)
                             && !neighbourMerges.RoadSegments.ContainsKey(change.RoadSegmentIdReference)
                             && _roadSegments.TryGetValue(change.RoadSegmentIdReference.RoadSegmentId, out var roadSegment)
                             && roadSegment is { IsRemoved: true, IsRemovedBecauseOfMigration: true })
            .Select(change => change.RoadSegmentIdReference.RoadSegmentId)
            .ToArray();

        foreach (var roadSegmentId in corrected)
        {
            context.Logger.LogInformation(
                "Road segment {RoadSegmentId} was removed by another inwinning and is collected again",
                roadSegmentId.ToInt32());
        }

        return corrected;
    }

    private static bool CanBeCollectedFromScratch(ModifyRoadSegmentChange change)
    {
        return change.Geometry is not null
               && change.GeometryDrawMethod is not null
               && change.Status is not null
               && change.AccessRestriction is not null
               && change.Category is not null
               && change.Morphology is not null
               && change.StreetNameId is not null
               && change.MaintenanceAuthorityId is not null
               && change.SurfaceType is not null
               && change.CarTrafficDirection is not null
               && change.BikeTrafficDirection is not null
               && change.PedestrianTrafficDirection is not null;
    }

    // Works out which of the road nodes this upload names were removed by an earlier inwinning and are to be brought
    // back.
    private IReadOnlyCollection<RoadNodeId> ResolveRoadNodeRemovalCorrections(
        RoadNetworkChanges changes,
        NeighbourMergeResolutions neighbourMerges,
        IReadOnlyCollection<RoadSegmentId> correctedRoadSegmentIds,
        ScopedRoadNetworkChangeContext context)
    {
        var removedRoadNodes = changes
            .OfType<ModifyRoadNodeChange>()
            // Everything the road node needs to be collected again has to be on the change: it is migrated from
            // scratch once it is back, and a migration without a geometry or a grensknoop is not one.
            .Where(change => change.Geometry is not null
                             && change.Grensknoop is not null
                             && !neighbourMerges.RoadNodeIds.Contains(change.RoadNodeId)
                             && _roadNodes.TryGetValue(change.RoadNodeId, out var roadNode)
                             && roadNode is { IsRemoved: true, IsRemovedBecauseOfMigration: true })
            .ToArray();

        if (removedRoadNodes.Length == 0)
        {
            return [];
        }

        // What the road node needs is a road running up to it once the changes are in. Where that road gets its number
        // from does not matter, so a road the upload draws anew counts as much as one the register already has - and it
        // has to, because an upload that delivers a road in pieces has them merged into a road segment that is added
        // rather than modified (see ScopedRoadNetwork-MergeRoadSegments.cs). Such a road was the only one running up to
        // road node 1059522 of the Lint delivery and to three of Wijnegem's, which is why they stayed removed and the
        // merged road segment was then refused for a missing start or end road node.
        //
        // The one road that does not count is a road segment that stays removed: that one is gone for good, and a road
        // node brought back for it would hang in the network with nothing attached, which the topology verification
        // rightly refuses. A road segment that is itself brought back does count - it will be there by the time the
        // road nodes are looked up.
        var collectedRoads = changes
            .OfType<ModifyRoadSegmentChange>()
            .Where(change => change.Geometry is not null
                             && _roadSegments.TryGetValue(change.RoadSegmentIdReference.RoadSegmentId, out var roadSegment)
                             && (!roadSegment.IsRemoved || correctedRoadSegmentIds.Contains(roadSegment.RoadSegmentId)))
            .Select(change => new CollectedRoad(change.RoadSegmentIdReference, change.Geometry!.Value, KnownToTheRegister: true))
            .Concat(changes
                .OfType<AddRoadSegmentChange>()
                .Select(change => new CollectedRoad(change.RoadSegmentIdReference, change.Geometry.Value, KnownToTheRegister: false)))
            .ToArray();

        var corrected = new List<RoadNodeId>();

        foreach (var removedRoadNode in removedRoadNodes)
        {
            var roadNodeCoordinate = removedRoadNode.Geometry!.Value.Coordinate;

            var runsToRoadNode = collectedRoads
                .FirstOrDefault(road => TouchesCoordinate(road.Geometry, roadNodeCoordinate));
            if (runsToRoadNode is null)
            {
                continue;
            }

            corrected.Add(removedRoadNode.RoadNodeId);

            context.Logger.LogInformation(
                "Road node {RoadNodeId} was removed by another inwinning and is collected again: {RoadSegment} runs to it",
                removedRoadNode.RoadNodeId.ToInt32(),
                runsToRoadNode.Describe());
        }

        return corrected;
    }
}
