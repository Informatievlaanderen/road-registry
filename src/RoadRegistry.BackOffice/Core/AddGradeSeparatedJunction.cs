namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using Messages;
using RoadRegistry.RoadSegment.ValueObjects;
using ValueObjects.Problems;

public class AddGradeSeparatedJunction : IRequestedChange
{
    public AddGradeSeparatedJunction(
        GradeSeparatedJunctionId id,
        GradeSeparatedJunctionId temporaryId,
        GradeSeparatedJunctionType type,
        RoadSegmentId upperSegmentId,
        RoadSegmentId? temporaryUpperSegmentId,
        RoadSegmentId lowerSegmentId,
        RoadSegmentId? temporaryLowerSegmentId)
    {
        Id = id;
        TemporaryId = temporaryId;
        Type = type ?? throw new ArgumentNullException(nameof(type));
        UpperSegmentId = upperSegmentId;
        TemporaryUpperSegmentId = temporaryUpperSegmentId;
        LowerSegmentId = lowerSegmentId;
        TemporaryLowerSegmentId = temporaryLowerSegmentId;
    }

    public GradeSeparatedJunctionId Id { get; }
    public RoadSegmentId LowerSegmentId { get; }
    public GradeSeparatedJunctionId TemporaryId { get; }
    public RoadSegmentId? TemporaryLowerSegmentId { get; }
    public RoadSegmentId? TemporaryUpperSegmentId { get; }
    public GradeSeparatedJunctionType Type { get; }
    public RoadSegmentId UpperSegmentId { get; }

    public IEnumerable<Messages.AcceptedChange> TranslateTo(CommandHandling.Actions.ChangeRoadNetwork.ValueObjects.Problem[] warnings)
    {
        yield return new Messages.AcceptedChange
        {
            Problems = warnings,
            GradeSeparatedJunctionAdded = new GradeSeparatedJunctionAdded
            {
                Id = Id,
                TemporaryId = TemporaryId,
                Type = Type.ToString(),
                UpperRoadSegmentId = UpperSegmentId,
                LowerRoadSegmentId = LowerSegmentId
            }
        };
    }

    public void TranslateToRejectedChange(Messages.RejectedChange message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        message.AddGradeSeparatedJunction = new Messages.AddGradeSeparatedJunction
        {
            TemporaryId = TemporaryId,
            Type = Type.ToString(),
            UpperSegmentId = TemporaryUpperSegmentId ?? UpperSegmentId,
            LowerSegmentId = TemporaryLowerSegmentId ?? LowerSegmentId
        };
    }

    public VerifyAfterResult VerifyAfter(AfterVerificationContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var problems = Problems.None;

        if (!context.AfterView.View.Segments.TryGetValue(UpperSegmentId, out var upperSegment)) problems = problems.Add(new UpperRoadSegmentMissing());

        if (!context.AfterView.View.Segments.TryGetValue(LowerSegmentId, out var lowerSegment)) problems = problems.Add(new LowerRoadSegmentMissing());

        if (upperSegment != null
            && lowerSegment != null
            && !upperSegment.Geometry.Intersects(lowerSegment.Geometry))
            problems = problems.Add(new UpperAndLowerRoadSegmentDoNotIntersect());

        return VerifyAfterResult.WithAcceptedChanges(problems, TranslateTo);
    }

    public Problems VerifyBefore(BeforeVerificationContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        return Problems.None;
    }
}
