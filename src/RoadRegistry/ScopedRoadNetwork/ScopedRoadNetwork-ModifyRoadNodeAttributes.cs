namespace RoadRegistry.ScopedRoadNetwork;

using System.Collections.Generic;
using System.Linq;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RoadRegistry.Extensions;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.ValueObjects.Problems;

public partial class ScopedRoadNetwork
{
    // 'Wijzig attribuutwaarden' for road nodes. Only the non-identifying attributes a caller may set are changed -
    // today that is 'grensknoop' alone, since the road node type is derived from the network and never named by hand.
    // The geometry is never touched, so the spatial indexes stay as they are and the network-wide verification of
    // AfterChangesApplied is not needed.
    //
    // The nodes are validated up front and the whole request is refused when any of them is wrong, so a request never
    // lands half-applied.
    public RoadNetworkChangeResult ModifyRoadNodeAttributes(
        IReadOnlyCollection<ModifyRoadNodeChange> changes,
        Provenance provenance,
        ILogger? logger = null)
    {
        logger ??= NullLogger.Instance;
        using var _ = logger.TimeAction();

        var context = new ScopedRoadNetworkChangeContext(this, provenance, logger);

        var problems = ValidateRoadNodes(changes);
        if (problems.HasError())
        {
            return new RoadNetworkChangeResult(Problems.None.AddRange(problems.Distinct()), context.Summary);
        }

        foreach (var change in changes)
        {
            problems += ModifyRoadNode(change, context);
        }

        // A value that is already what the request asks for is no change at all: ModifyRoadNode records nothing, and
        // then there is nothing to summarise either.
        if (!problems.HasError() && context.Summary.HasChanges())
        {
            ApplyChangeSummary(context, provenance);
        }

        return new RoadNetworkChangeResult(Problems.None.AddRange(problems.Distinct()), context.Summary);
    }

    private Problems ValidateRoadNodes(IReadOnlyCollection<ModifyRoadNodeChange> changes)
    {
        var problems = Problems.None;
        var validatedRoadNodeIds = new HashSet<RoadNodeId>();

        foreach (var change in changes)
        {
            // VAL-4
            if (!_roadNodes.TryGetValue(change.RoadNodeId, out var roadNode) || roadNode.IsRemoved)
            {
                problems += roadNode is null
                    ? new RoadNodeChangeAttributesNotFound(change.RoadNodeId)
                    : (Error)new RoadNodeChangeAttributesIsRemoved(change.RoadNodeId);
                continue;
            }

            // VAL-5
            if (roadNode.IsRemoved)
            {
                problems += new RoadNodeChangeAttributesIsRemoved(change.RoadNodeId);
                continue;
            }

            // A road node that has not completed its inwinning is still a V1 node: it carries no type, and 'grensknoop'
            // is a V2 attribute. Setting it would record an attribute on something that is not a V2 road node yet - the
            // same reason every other editing action refuses one, ModifyRoadSegmentAttributes included.
            problems += ValidateRoadNodesHaveCompletedInwinning([change.RoadNodeId]);

            validatedRoadNodeIds.Add(change.RoadNodeId);
        }

        // VAL-10, which GAWR-7731 numbers among the inwinning validations it states for the edit endpoints as a whole:
        // every road segment meeting at a road node named has to have completed its inwinning as well. The road node
        // is only edited where the roads around it are the register's to speak for: a node at the edge of what has
        // been collected is one the inwinning may still reshape - it merges roads across a municipality border and
        // removes the node they met at - so what is recorded on it now would be recorded on a node that is about to
        // be redrawn. Asked for the segments of the road nodes that got this far only, so a request naming a road node
        // that does not exist is answered with that and not with the roads around the ones that do.
        //
        // Reported as the road segment's problem, the same as every other action that refuses a road whose inwinning
        // is not finished.
        problems += ValidateRoadSegmentsHaveCompletedInwinning(
            GetNonRemovedRoadSegments()
                .Where(x => x.GetNodeIds().Any(validatedRoadNodeIds.Contains))
                .Select(x => x.RoadSegmentId));

        return problems;
    }
}
