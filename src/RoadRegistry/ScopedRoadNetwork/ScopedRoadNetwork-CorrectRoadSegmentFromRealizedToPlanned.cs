namespace RoadRegistry.ScopedRoadNetwork;

using System.Collections.Generic;
using System.Linq;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RoadRegistry.Extensions;
using RoadRegistry.GradeSeparatedJunction.Changes;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.ValueObjects;
using RoadRegistry.ValueObjects.Problems;

public partial class ScopedRoadNetwork
{
    // 'Corrigeer een gerealiseerd wegsegment naar gepland'.
    //
    // The mirror image of realizing: only a realized segment is knotted into the network, so correcting one back to
    // 'gepland' unhooks it. It gives up its two road nodes, the crossings it took part in go with it, and each node it
    // hung off is either removed - nothing else was using it - or re-typed for what is left.
    //
    // The geometry and every attribute stay exactly as they are: the segment is still drawn where it was drawn, it
    // simply is not part of the network any more.
    public RoadNetworkChangeResult CorrectRoadSegmentFromRealizedToPlanned(
        RoadSegmentId roadSegmentId,
        bool mayModifyMeasuredRoadSegments,
        IRoadNetworkIdGenerator idGenerator,
        Provenance provenance,
        ILogger? logger = null)
    {
        logger ??= NullLogger.Instance;
        using var _ = logger.TimeAction();

        var context = new ScopedRoadNetworkChangeContext(this, provenance, logger);

        var problems = Problems.None;
        var roadSegmentContext = Problems.WithContext(roadSegmentId);

        // VAL-2, VAL-3
        if (!_roadSegments.TryGetValue(roadSegmentId, out var roadSegment) || roadSegment.IsRemoved)
        {
            return Failed(roadSegmentContext + new RoadSegmentNotFound(), context);
        }

        // Re-validate what the API also checks: the request may have gone stale between being accepted and handled.
        if (!roadSegment.HasMigrated())
        {
            problems += roadSegmentContext + new RoadSegmentNotCompletedInwinning();
        }
        // VAL-4
        if (roadSegment.Status != RoadSegmentStatusV2.Gerealiseerd)
        {
            problems += roadSegmentContext + new RoadSegmentCorrectFromRealizedToPlannedStatusNotValid();
        }
        // VAL-5
        if (!mayModifyMeasuredRoadSegments && roadSegment.Attributes?.GeometryDrawMethod == RoadSegmentGeometryDrawMethodV2.Ingemeten)
        {
            problems += roadSegmentContext + new RoadSegmentMeasuredNotAllowed();
        }
        if (problems.HasError())
        {
            return Failed(problems, context);
        }

        RebuildSpatialIndexes(logger);

        // Held on to before the segment lets go of them: once it is planned it no longer says which nodes it hung off.
        var previousRoadNodeIds = roadSegment.GetNodeIds().ToArray();

        problems += roadSegment.CorrectFromRealizedToPlanned(context);
        if (problems.HasError())
        {
            return Failed(problems, context);
        }
        context.Summary.RoadSegments.Modified.Add(roadSegmentId);

        // A crossing is a statement about two realized roads. This one is not realized any more, so whatever recorded
        // its crossings goes with it - grade and grade separated alike.
        problems += TryToRemoveLinkedGradeJunctions(roadSegmentId, context);
        problems += TryToRemoveLinkedGradeSeparatedJunctions(roadSegmentId, context);
        if (problems.HasError())
        {
            return Failed(problems, context);
        }

        problems += UnhookRoadNodes(previousRoadNodeIds, idGenerator, context);

        if (!problems.HasError() && context.Summary.HasChanges())
        {
            ApplyChangeSummary(context, provenance);
        }

        return new RoadNetworkChangeResult(Problems.None.AddRange(problems.Distinct()), context.Summary);
    }

    // A node the segment came loose from is either not holding anything up any more, in which case it goes, or still
    // carries other segments and is re-typed for what is left of them.
    private Problems UnhookRoadNodes(IReadOnlyCollection<RoadNodeId> roadNodeIds, IRoadNetworkIdGenerator idGenerator, ScopedRoadNetworkChangeContext context)
    {
        var problems = Problems.None;

        foreach (var roadNodeId in roadNodeIds)
        {
            if (!_roadNodes.TryGetValue(roadNodeId, out var roadNode) || roadNode.IsRemoved)
            {
                continue;
            }

            var remainingSegments = GetNonRemovedRoadSegments()
                .Count(x => x.StartNodeId == roadNodeId || x.EndNodeId == roadNodeId);
            if (remainingSegments == 0)
            {
                problems += RemoveRoadNode(roadNodeId, context);
                continue;
            }

            // Merging is off for the same reason as when realizing: this action names one segment, and the roads that
            // happened to meet it keep their own identity.
            problems += roadNode.VerifyTopologyAndUpdateType(_roadSegmentsSpatialIndex, idGenerator, context, mayMergeRoadSegments: false);
            if (roadNode.HasChanges())
            {
                context.Summary.RoadNodes.Modified.Add(roadNodeId);
            }
        }

        return problems;
    }
}
