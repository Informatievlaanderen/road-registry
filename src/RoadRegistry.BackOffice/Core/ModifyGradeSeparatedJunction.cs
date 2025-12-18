namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using Messages;
using RoadRegistry.RoadSegment.ValueObjects;
using ValueObjects.Problems;
using Problem = RoadRegistry.Infrastructure.Messages.Problem;

public class ModifyGradeSeparatedJunction : IRequestedChange
{
    public ModifyGradeSeparatedJunction(
        GradeSeparatedJunctionId id,
        GradeSeparatedJunctionType type,
        RoadSegmentId upperSegmentId,
        RoadSegmentId? temporaryUpperSegmentId,
        RoadSegmentId lowerSegmentId,
        RoadSegmentId? temporaryLowerSegmentId)
    {
        Id = id;
        Type = type ?? throw new ArgumentNullException(nameof(type));
        UpperSegmentId = upperSegmentId;
        TemporaryUpperSegmentId = temporaryUpperSegmentId;
        LowerSegmentId = lowerSegmentId;
        TemporaryLowerSegmentId = temporaryLowerSegmentId;
    }

    public GradeSeparatedJunctionId Id { get; }
    public GradeSeparatedJunctionType Type { get; }
    public RoadSegmentId UpperSegmentId { get; }
    public RoadSegmentId? TemporaryUpperSegmentId { get; }
    public RoadSegmentId LowerSegmentId { get; }
    public RoadSegmentId? TemporaryLowerSegmentId { get; }

    public IEnumerable<Messages.AcceptedChange> TranslateTo(Problem[] warnings)
    {
        yield return new Messages.AcceptedChange
        {
            Problems = warnings,
            GradeSeparatedJunctionModified = new GradeSeparatedJunctionModified
            {
                Id = Id,
                Type = Type.ToString(),
                UpperRoadSegmentId = UpperSegmentId,
                LowerRoadSegmentId = LowerSegmentId
            }
        };
    }

    public void TranslateToRejectedChange(Messages.RejectedChange message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        message.ModifyGradeSeparatedJunction = new Messages.ModifyGradeSeparatedJunction
        {
            Id = Id,
            Type = Type.ToString(),
            UpperSegmentId = TemporaryUpperSegmentId ?? UpperSegmentId,
            LowerSegmentId = TemporaryLowerSegmentId ?? LowerSegmentId
        };
    }

    public VerifyAfterResult VerifyAfter(AfterVerificationContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var problems = Problems.None;

        if (!context.AfterView.View.Segments.TryGetValue(UpperSegmentId, out var upperSegment))
        {
            problems = problems.Add(new UpperRoadSegmentMissing());
        }

        if (!context.AfterView.View.Segments.TryGetValue(LowerSegmentId, out var lowerSegment))
        {
            problems = problems.Add(new LowerRoadSegmentMissing());
        }

        if (upperSegment != null
            && lowerSegment != null
            && !upperSegment.Geometry.Intersects(lowerSegment.Geometry))
        {
            problems = problems.Add(new UpperAndLowerRoadSegmentDoNotIntersect());
        }

        return new VerifyAfterResult(problems);
    }

    public Problems VerifyBefore(BeforeVerificationContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var problems = Problems.None;

        if (!context.BeforeView.View.GradeSeparatedJunctions.ContainsKey(Id))
        {
            problems = problems.Add(new GradeSeparatedJunctionNotFound());
        }

        return problems;
    }
}
