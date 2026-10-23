namespace RoadRegistry.ScopedRoadNetwork;

using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;

public partial class ScopedRoadNetwork
{
    // The other half of the neighbouring-municipality story (see ScopedRoadNetwork-Migrate-NeighbourMerge.cs). There,
    // the municipality that uploaded first had merged a road across the border and taken the road node with it. Here it
    // removed the road node outright, along with the road segment that ran to it, and the neighbour still brings that
    // road node together with a road segment of its own - one that lies entirely on its own side and that it is
    // entitled to collect.
    //
    // Refusing it would leave the second municipality unable to collect a road it owns because of what its neighbour
    // did to a road node on the border. So the road node comes back instead, under the identifier it always had.

    // Works out which of the road nodes this upload names were removed by an earlier inwinning and are to be brought
    // back. Run after ResolveNeighbourMerges and given its answer: a road node that merge accounts for is not this,
    // and the two never claim the same one anyway - a merge removes the road node it no longer needs with
    // RoadNodeWasRemoved, which is not a removal any inwinning may undo.
    private IReadOnlyCollection<RoadNodeId> ResolveRemovalCorrections(
        RoadNetworkChanges changes,
        NeighbourMergeResolutions neighbourMerges,
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

        // Only a road segment the road network already has counts. One the upload draws from scratch has no
        // identifier of its own yet, and a road the neighbour removed along with the road node is gone for good:
        // bringing the road node back on its own would leave it hanging in the network with nothing attached, which
        // the topology verification rightly refuses.
        var roadSegmentChanges = changes
            .OfType<ModifyRoadSegmentChange>()
            .Where(change => change.Geometry is not null
                             && _roadSegments.TryGetValue(change.RoadSegmentIdReference.RoadSegmentId, out var roadSegment)
                             && !roadSegment.IsRemoved)
            .ToArray();

        var corrected = new List<RoadNodeId>();

        foreach (var removedRoadNode in removedRoadNodes)
        {
            var roadNodeCoordinate = removedRoadNode.Geometry!.Value.Coordinate;

            var runsToRoadNode = roadSegmentChanges
                .FirstOrDefault(change => TouchesCoordinate(change.Geometry!.Value, roadNodeCoordinate));
            if (runsToRoadNode is null)
            {
                continue;
            }

            corrected.Add(removedRoadNode.RoadNodeId);

            context.Logger.LogInformation(
                "Road node {RoadNodeId} was removed by another inwinning and is collected again: road segment {RoadSegmentId} runs to it",
                removedRoadNode.RoadNodeId.ToInt32(),
                runsToRoadNode.RoadSegmentIdReference.RoadSegmentId.ToInt32());
        }

        return corrected;
    }
}
