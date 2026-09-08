namespace RoadRegistry.ScopedRoadNetwork;

using System;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RoadRegistry.Extensions;
using RoadRegistry.GradeJunction;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.ValueObjects.Problems;

public partial class ScopedRoadNetwork
{
    // 'Wijzig ongelijkgrondse kruising naar gelijkgrondse kruising'. The mirror of the other direction, and the rarer
    // one: a bridge or tunnel disappears from the terrain, or a grade junction was turned into a grade separated one
    // in error. Nothing is named in the request beyond the junction itself - a grade junction says nothing about which
    // road passes over the other, so there is nothing left for the caller to decide.
    public RoadNetworkChangeResult ChangeGradeSeparatedJunctionToGradeJunction(
        GradeSeparatedJunctionId gradeSeparatedJunctionId,
        IRoadNetworkIdGenerator idGenerator,
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
        var inwinningProblems = ValidateRoadSegmentsHaveCompletedInwinning([gradeSeparatedJunction.LowerRoadSegmentId, gradeSeparatedJunction.UpperRoadSegmentId]);
        if (inwinningProblems.HasError())
        {
            return new RoadNetworkChangeResult(inwinningProblems, context.Summary);
        }

        // The crossing point itself does not move, so the new junction takes the geometry the old one carried. A
        // junction without one cannot be turned into anything: it is recorded when the crossing is found, so its
        // absence is a corrupt aggregate rather than something the caller did.
        var geometry = gradeSeparatedJunction.Geometry
                       ?? throw new InvalidOperationException($"Grade separated junction {gradeSeparatedJunctionId} has no geometry to hand over to a grade junction. This should not happen.");

        var gradeJunction = GradeJunction.AddBecauseOfGradeSeparatedJunctionChange(
            gradeSeparatedJunctionId,
            gradeSeparatedJunction.LowerRoadSegmentId,
            gradeSeparatedJunction.UpperRoadSegmentId,
            geometry,
            provenance,
            idGenerator,
            context.OrdinalProvider);

        _gradeJunctions.Add(gradeJunction.GradeJunctionId, gradeJunction);
        context.Summary.GradeJunctions.Added.Add(gradeJunction.GradeJunctionId);

        var problems = gradeSeparatedJunction.ChangeToGradeJunction(gradeJunction.GradeJunctionId, provenance);
        if (problems.HasError())
        {
            return new RoadNetworkChangeResult(problems, context.Summary);
        }

        context.Summary.GradeSeparatedJunctions.Removed.Add(gradeSeparatedJunctionId);

        ApplyChangeSummary(context, provenance);

        return new RoadNetworkChangeResult(problems, context.Summary);
    }
}
