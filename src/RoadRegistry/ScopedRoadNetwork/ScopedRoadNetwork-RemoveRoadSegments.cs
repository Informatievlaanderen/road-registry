namespace RoadRegistry.ScopedRoadNetwork;

using System;
using System.Collections.Generic;
using System.Linq;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.ValueObjects.Problems;

public partial class ScopedRoadNetwork
{
    // Removing road segments is historeren taken one step further: the road nodes the segment hung off are re-typed or
    // removed and the crossings it took part in go with it, by the same rules, and on top of that a node that is left
    // holding two segments and is no longer needed takes them with it - the two are merged, and the validatieknoop
    // between them disappears.
    //
    // Which segments may go is not for the register to decide. This action belongs to a central manager, so no road
    // segment category is off limits, and an island left behind is allowed - both are the caller's business.
    public void RemoveRoadSegments(
        IReadOnlyCollection<RoadSegmentId> roadSegmentIds,
        IRoadNetworkIdGenerator idGenerator,
        Provenance provenance,
        ILogger? logger = null)
    {
        logger ??= NullLogger.Instance;

        var context = new ScopedRoadNetworkChangeContext(this, provenance, logger);
        var problems = Problems.None;
        var affectedRoadNodeIds = new List<RoadNodeId>();

        foreach (var roadSegmentId in roadSegmentIds)
        {
            problems += TryRemoveRoadSegment(roadSegmentId, context, affectedRoadNodeIds);
        }

        // Reported for every segment the request names rather than for the first one that is wrong, so a caller
        // removing a thousand of them learns about all of them at once.
        problems.ThrowIfError();

        // The road nodes are only looked at once every named segment is gone. Doing it per segment would let a bulk
        // removal merge two segments across a node and then remove one half of that merge again: what is left of the
        // network says something about a node only after the whole request has been taken out of it.
        RebuildSpatialIndexes(logger);

        foreach (var roadNodeId in affectedRoadNodeIds.Distinct())
        {
            problems += FixRoadNodeAfterRemoval(roadNodeId, idGenerator, context);
        }

        problems.ThrowIfError();

        // What the ticket reports back. A request naming only segments the network no longer has changed nothing, and
        // an event saying that is worse than no event: the projections would have a summary to apply with nothing in
        // it, and the ticket would report a change that never happened.
        if (context.Summary.HasChanges())
        {
            ApplyChangeSummary(context, provenance);
        }
    }

    private Problems TryRemoveRoadSegment(
        RoadSegmentId roadSegmentId,
        ScopedRoadNetworkChangeContext context,
        ICollection<RoadNodeId> affectedRoadNodeIds)
    {
        var problems = Problems.WithContext(roadSegmentId);

        // A segment the network does not have, or one that is already gone, is what the caller asked for: there is
        // nothing left to remove and nothing to report. Naming it changes nothing, and a request sent twice does not
        // fail the second time.
        if (!_roadSegments.TryGetValue(roadSegmentId, out var segment) || segment.IsRemoved)
        {
            return Problems.None;
        }

        // A segment whose inwinning is not finished is not a V2 segment yet: it carries no attributes, so what is left
        // at its nodes cannot be derived and it is not this action's to remove. Every other editing action refuses it
        // for the same reason.
        if (!segment.HasMigrated())
        {
            return problems + new RoadSegmentNotCompletedInwinning();
        }

        // Held on to before the segment lets go of them: once it is removed it no longer says which nodes it hung off.
        // Only a realized segment is knotted into the network, so only that one leaves nodes behind.
        var previousRoadNodeIds = segment.Status == RoadSegmentStatusV2.Gerealiseerd
            ? segment.GetNodeIds().ToArray()
            : [];

        // Removing the segment is only half of it: FixRoadNodeAfterRemoval then re-derives the type of every node it
        // hung off, or removes it. A node whose inwinning is not finished has no type to re-derive - deriving one would
        // migrate it as a side effect of removing a segment - so it is refused here, before anything is taken out.
        var roadNodeProblems = ValidateRoadNodesHaveCompletedInwinning(previousRoadNodeIds);
        if (roadNodeProblems.HasError())
        {
            return problems + roadNodeProblems;
        }

        problems += segment.Remove(context.Provenance);
        if (problems.HasError())
        {
            return problems;
        }

        context.Summary.RoadSegments.Removed.Add(roadSegmentId);

        // A crossing is a statement about two realized roads and this one is gone, so whatever recorded its crossings
        // goes with it - grade and grade separated alike, the same as when a segment is historeerd.
        problems += TryToRemoveLinkedGradeJunctions(roadSegmentId, context);
        problems += TryToRemoveLinkedGradeSeparatedJunctions(roadSegmentId, context);

        foreach (var roadNodeId in previousRoadNodeIds)
        {
            affectedRoadNodeIds.Add(roadNodeId);
        }

        return problems;
    }

    // What the removal leaves behind at a road node: one that is holding nothing up any more goes, and otherwise its
    // type is re-derived from what is left of the network. That derivation is where a node holding two segments that
    // does not need to be there merges them - the 'samenvoegen' step - so this needs no rules of its own.
    private Problems FixRoadNodeAfterRemoval(
        RoadNodeId roadNodeId,
        IRoadNetworkIdGenerator idGenerator,
        ScopedRoadNetworkChangeContext context)
    {
        // A realized segment hangs off two road nodes and the network is loaded with them, and nothing in this action
        // removes a node other than the one it is looking at. A node named by the segment that the network does not
        // have, or one that is already gone, is therefore not something the caller did: the aggregate is wrong. Saying
        // nothing would leave a node behind carrying a type that no longer matches what hangs off it.
        if (!_roadNodes.TryGetValue(roadNodeId, out var roadNode))
        {
            throw new InvalidOperationException($"Road node {roadNodeId} is connected to road segments but is not part of the road network. This should not happen.");
        }

        if (roadNode.IsRemoved)
        {
            throw new InvalidOperationException($"Road node {roadNodeId} is connected to road segments but is already removed. This should not happen.");
        }

        var remainingSegments = GetNonRemovedRoadSegments()
            .Count(x => x.StartNodeId == roadNodeId || x.EndNodeId == roadNodeId);
        if (remainingSegments == 0)
        {
            return RemoveRoadNode(roadNodeId, context);
        }

        // Merging is on: two segments left at a node that is no longer needed become one, which is the
        // 'samenvoegen' step the story asks for.
        var problems = roadNode.VerifyTopologyAndUpdateType(_roadSegmentsSpatialIndex, idGenerator, context, mayMergeRoadSegments: true);

        // A node that was re-typed is a change of its own and the ticket has to say so. Not when it was merged away:
        // it changed too, but RemoveRoadNode already reported that, and it is removed rather than modified.
        if (!roadNode.IsRemoved && roadNode.HasChanges())
        {
            context.Summary.RoadNodes.Modified.Add(roadNodeId);
        }

        return problems;
    }
}
