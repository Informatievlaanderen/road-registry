namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using Messages;
using RoadRegistry.Extensions;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ValueObjects.ProblemCodes;
using ValueObjects.Problems;
using Problem = RoadRegistry.Infrastructure.Messages.Problem;

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

    public IEnumerable<Messages.AcceptedChange> TranslateTo(Problem[] warnings)
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

        var problems = Problems.None.WithContext(ProblemContext.For(TemporaryId));

        if (!context.AfterView.View.Segments.TryGetValue(UpperSegmentId, out var upperSegment))
        {
            problems += new Error(ProblemCode.GradeSeparatedJunction.UpperSegmentMissing.ToString(),
                new RoadSegmentIdReference(context.Translator.TranslateToOriginalOrTemporaryOrId(TemporaryUpperSegmentId ?? UpperSegmentId))
                    .ToRoadSegmentProblemParameters().ToArray());
        }

        if (!context.AfterView.View.Segments.TryGetValue(LowerSegmentId, out var lowerSegment))
        {
            problems += new Error(ProblemCode.GradeSeparatedJunction.LowerSegmentMissing.ToString(),
                new RoadSegmentIdReference(context.Translator.TranslateToOriginalOrTemporaryOrId(TemporaryLowerSegmentId ?? LowerSegmentId))
                    .ToRoadSegmentProblemParameters().ToArray());
        }

        if (upperSegment != null
            && lowerSegment != null
            && !upperSegment.Geometry.Intersects(lowerSegment.Geometry))
        {
            problems += new Error(ProblemCode.GradeSeparatedJunction.UpperAndLowerDoNotIntersect.ToString(), Enumerable.Empty<ProblemParameter>()
                .Concat(new RoadSegmentIdReference(context.Translator.TranslateToOriginalOrTemporaryOrId(TemporaryLowerSegmentId ?? LowerSegmentId)).ToRoadSegmentProblemParameters("LowerWegsegment"))
                .Concat(new RoadSegmentIdReference(context.Translator.TranslateToOriginalOrTemporaryOrId(TemporaryUpperSegmentId ?? UpperSegmentId)).ToRoadSegmentProblemParameters("UpperWegsegment"))
                .ToArray());
        }

        return VerifyAfterResult.WithAcceptedChanges(problems, TranslateTo);
    }

    public Problems VerifyBefore(BeforeVerificationContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        return Problems.None;
    }
}
