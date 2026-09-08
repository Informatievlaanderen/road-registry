namespace RoadRegistry.ScopedRoadNetwork;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RoadRegistry.Extensions;
using RoadRegistry.GradeSeparatedJunction.Changes;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.ValueObjects.Problems;

public partial class ScopedRoadNetwork
{
    // 'Wijzig attribuutwaarde(n) voor een ongelijkgrondse kruising'. Recording a bridge or a tunnel can go wrong in
    // two ways - the roads end up the wrong way round, or the kind is wrong - and this corrects both in one action.
    // The crossing itself is not touched: which roads meet there and where they meet stays what it was.
    public RoadNetworkChangeResult ModifyGradeSeparatedJunctionAttributes(
        GradeSeparatedJunctionId gradeSeparatedJunctionId,
        RoadSegmentId lowerRoadSegmentId,
        RoadSegmentId upperRoadSegmentId,
        GradeSeparatedJunctionTypeV2 type,
        Provenance provenance,
        ILogger? logger = null)
    {
        logger ??= NullLogger.Instance;
        using var _ = logger.TimeAction();

        var context = new ScopedRoadNetworkChangeContext(this, provenance, logger);

        // VAL-2
        if (!_gradeSeparatedJunctions.TryGetValue(gradeSeparatedJunctionId, out var gradeSeparatedJunction))
        {
            return Failed(new GradeSeparatedJunctionDoesNotExist(gradeSeparatedJunctionId), context);
        }

        // VAL-3
        if (gradeSeparatedJunction.IsRemoved)
        {
            return Failed(new GradeSeparatedJunctionIsRemoved(gradeSeparatedJunctionId), context);
        }

        // The crossing is about two roads, and a road whose inwinning is not finished is not one this register can
        // say anything about yet.
        var problems = ValidateRoadSegmentsHaveCompletedInwinning([gradeSeparatedJunction.LowerRoadSegmentId, gradeSeparatedJunction.UpperRoadSegmentId]);
        if (problems.HasError())
        {
            return new RoadNetworkChangeResult(problems, context.Summary);
        }

        // VAL-5, VAL-7: swapping the two roads is what this action is for, so both still have to be roads the crossing
        // is about. Naming a third road is not a correction, it is a different crossing.
        if (!gradeSeparatedJunction.IsConnectedTo(lowerRoadSegmentId))
        {
            problems += new RoadSegmentDoesNotBelongToGradeSeparatedJunction(lowerRoadSegmentId, gradeSeparatedJunctionId);
        }

        if (!gradeSeparatedJunction.IsConnectedTo(upperRoadSegmentId))
        {
            problems += new RoadSegmentDoesNotBelongToGradeSeparatedJunction(upperRoadSegmentId, gradeSeparatedJunctionId);
        }

        // VAL-8
        if (lowerRoadSegmentId == upperRoadSegmentId)
        {
            problems += new GradeSeparatedJunctionUpperEqualsLowerRoadSegment();
        }

        if (problems.HasError())
        {
            return new RoadNetworkChangeResult(problems, context.Summary);
        }

        // Asking for what is already recorded is no correction: no event, and so no new version date either.
        if (gradeSeparatedJunction.LowerRoadSegmentId == lowerRoadSegmentId
            && gradeSeparatedJunction.UpperRoadSegmentId == upperRoadSegmentId
            && gradeSeparatedJunction.Type == type)
        {
            return new RoadNetworkChangeResult(problems, context.Summary);
        }

        problems += ModifyGradeSeparatedJunction(new ModifyGradeSeparatedJunctionChange
        {
            GradeSeparatedJunctionId = gradeSeparatedJunctionId,
            LowerRoadSegmentId = lowerRoadSegmentId,
            UpperRoadSegmentId = upperRoadSegmentId,
            Type = type
        }, context);
        if (problems.HasError())
        {
            return new RoadNetworkChangeResult(problems, context.Summary);
        }

        ApplyChangeSummary(context, provenance);

        return new RoadNetworkChangeResult(problems, context.Summary);
    }
}
