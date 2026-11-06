namespace RoadRegistry.ScopedRoadNetwork;

using System.Collections.Generic;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.ValueObjects;
using RoadRegistry.ValueObjects.Problems;

public partial class ScopedRoadNetwork
{
    // An upload is built against the extract the municipality downloaded, and that extract is a snapshot: a road node
    // another inwinning added on that very point afterwards is not in it. The feature compare matches road nodes on
    // geometry against the extract, so it has nothing to match and asks for the road node to be added.
    //
    // Adding it would put two road nodes on one and the same point, which goes wrong twice over. The topology
    // verification reports the geometry as taken, from both sides. And the road segments, which resolve their start
    // and end node by geometry rather than by the uploaded WK_OIDN, all hang off whichever of the two the spatial
    // index happens to return first - leaving the other one connected to nothing at all.
    //
    // So the road node that is already there is the one used: the uploaded identifier points at it, and what the
    // upload says about it is applied to it. The road segments of this upload then meet, at that road node, the ones
    // that are already on record - and the topology verification afterwards treats it like any other road node,
    // including merging the two road segments meeting there when the road node turns out not to be needed.
    //
    // This is the mirror image of what a neighbouring inwinning merging a road node away asks for; see
    // ScopedRoadNetwork-Migrate-NeighbourMerge.cs (GAWR-7847, GAWR-7826).
    //
    // Looked up through the very same FindRoadNode the road segments use, so the two cannot disagree about which
    // road node sits on a point.
    private RoadNode.RoadNode? FindRoadNodeAlreadyThere(AddRoadNodeChange change, ScopedRoadNetworkChangeContext context)
    {
        var roadNode = FindRoadNode(change.Geometry.Value.Coordinate);

        // Only a road node that was already there. Two road nodes on one point within a single upload is the
        // uploader's own mistake, and stays reported as such.
        return roadNode is not null && !context.Summary.RoadNodes.Added.Contains(roadNode.RoadNodeId)
            ? roadNode
            : null;
    }

    // What the upload asked to add, said about the road node that is already there. The uploaded WK_OIDN is carried
    // as the original identifier, the same as for a road node the feature compare did match: every problem about this
    // road node then names the identifier the road network uses as well as the one the uploader knows it by.
    private static ModifyRoadNodeChange AsModifyChange(AddRoadNodeChange change, RoadNodeId roadNodeId, ScopedRoadNetworkChangeContext context)
    {
        context.IdTranslator.RegisterOriginalId(change.OriginalId ?? change.TemporaryId, roadNodeId);

        return new ModifyRoadNodeChange
        {
            RoadNodeId = roadNodeId,
            OriginalId = change.OriginalId ?? change.TemporaryId,
            Geometry = change.Geometry,
            Grensknoop = change.Grensknoop
        };
    }

    // The upload path of Change: a road node that is already there is modified instead of added.
    private Problems AddRoadNodeFromUpload(AddRoadNodeChange change, IRoadNetworkIdGenerator idGenerator, ScopedRoadNetworkChangeContext context)
    {
        var roadNodeAlreadyThere = FindRoadNodeAlreadyThere(change, context);

        return roadNodeAlreadyThere is null
            ? AddRoadNode(change, idGenerator, context)
            : ModifyRoadNode(AsModifyChange(change, roadNodeAlreadyThere.RoadNodeId, context), context);
    }

    // The same for the inwinning, which goes through MigrateRoadNode rather than ModifyRoadNode: a road node that is
    // already there may still be a V1 one that has its own collecting to do.
    private Problems AddRoadNodeFromInwinning(AddRoadNodeChange change, NeighbourMergeResolutions neighbourMerges, IReadOnlyCollection<RoadNodeId> removalCorrections, IRoadNetworkIdGenerator idGenerator, ScopedRoadNetworkChangeContext context)
    {
        var roadNodeAlreadyThere = FindRoadNodeAlreadyThere(change, context);

        return roadNodeAlreadyThere is null
            ? AddRoadNode(change, idGenerator, context)
            : MigrateRoadNode(AsModifyChange(change, roadNodeAlreadyThere.RoadNodeId, context), neighbourMerges, removalCorrections, context);
    }
}
