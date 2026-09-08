namespace RoadRegistry.ScopedRoadNetwork;

using System;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RoadRegistry.Extensions;
using RoadRegistry.GradeSeparatedJunction;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.ValueObjects.Problems;

public partial class ScopedRoadNetwork
{
    // 'Wijzig gelijkgrondse kruising naar ongelijkgrondse kruising'. Realizing a road segment records a grade junction
    // wherever it crosses another road, but a crossing can turn out to be a bridge or a tunnel. This is one action, not
    // a removal followed by an addition: the crossing does not go away, only what is recorded about it changes, and the
    // events say so.
    //
    // Which of the two roads goes under and which goes over is the caller's to say - the register cannot tell from the
    // geometry - so both are named in the request, and both have to be roads this crossing is actually about.
    public RoadNetworkChangeResult ChangeGradeJunctionToGradeSeparatedJunction(
        GradeJunctionId gradeJunctionId,
        RoadSegmentId lowerRoadSegmentId,
        RoadSegmentId upperRoadSegmentId,
        GradeSeparatedJunctionTypeV2 type,
        IRoadNetworkIdGenerator idGenerator,
        Provenance provenance,
        ILogger? logger = null)
    {
        logger ??= NullLogger.Instance;
        using var _ = logger.TimeAction();

        var context = new ScopedRoadNetworkChangeContext(this, provenance, logger);

        // VAL-2
        if (!_gradeJunctions.TryGetValue(gradeJunctionId, out var gradeJunction))
        {
            return Failed(new GradeJunctionDoesNotExist(gradeJunctionId), context);
        }

        // VAL-3
        if (gradeJunction.IsRemoved)
        {
            return Failed(new GradeJunctionIsRemoved(gradeJunctionId), context);
        }

        // The crossing is about two roads, and a road whose inwinning is not finished is not one this register can
        // say anything about yet - not even that it passes under or over another.
        var problems = ValidateRoadSegmentsHaveCompletedInwinning([gradeJunction.RoadSegmentId1, gradeJunction.RoadSegmentId2]);
        if (problems.HasError())
        {
            return new RoadNetworkChangeResult(problems, context.Summary);
        }

        // VAL-5, VAL-7
        if (!gradeJunction.IsConnectedTo(lowerRoadSegmentId))
        {
            problems += new RoadSegmentDoesNotBelongToGradeJunction(lowerRoadSegmentId, gradeJunctionId);
        }

        if (!gradeJunction.IsConnectedTo(upperRoadSegmentId))
        {
            problems += new RoadSegmentDoesNotBelongToGradeJunction(upperRoadSegmentId, gradeJunctionId);
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

        // The crossing point itself does not move, so the new junction takes the geometry the old one carried. A grade
        // junction without one cannot be turned into anything: it is recorded when the crossing is found, so its
        // absence is a corrupt aggregate rather than something the caller did.
        var geometry = gradeJunction.Geometry
                       ?? throw new InvalidOperationException($"Grade junction {gradeJunctionId} has no geometry to hand over to a grade separated junction. This should not happen.");

        var gradeSeparatedJunction = GradeSeparatedJunction.AddBecauseOfGradeJunctionChange(
            gradeJunctionId,
            lowerRoadSegmentId,
            upperRoadSegmentId,
            type,
            geometry,
            provenance,
            idGenerator,
            context.OrdinalProvider);

        _gradeSeparatedJunctions.Add(gradeSeparatedJunction.GradeSeparatedJunctionId, gradeSeparatedJunction);
        context.Summary.GradeSeparatedJunctions.Added.Add(gradeSeparatedJunction.GradeSeparatedJunctionId);

        problems += gradeJunction.ChangeToGradeSeparatedJunction(gradeSeparatedJunction.GradeSeparatedJunctionId, provenance);
        if (problems.HasError())
        {
            return new RoadNetworkChangeResult(problems, context.Summary);
        }

        context.Summary.GradeJunctions.Removed.Add(gradeJunctionId);

        ApplyChangeSummary(context, provenance);

        return new RoadNetworkChangeResult(problems, context.Summary);
    }

    // Nothing was changed, so the summary stays empty and no summary event is raised.
    private static RoadNetworkChangeResult Failed(Error error, ScopedRoadNetworkChangeContext context)
    {
        return new RoadNetworkChangeResult(Problems.Single(error), context.Summary);
    }
}
