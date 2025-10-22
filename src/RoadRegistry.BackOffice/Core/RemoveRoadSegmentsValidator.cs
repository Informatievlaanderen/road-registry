namespace RoadRegistry.BackOffice.Core;

using Extensions;
using FluentValidation;
using ProblemCodes;
using RoadRegistry.RoadSegment.ValueObjects;

public class RemoveRoadSegmentsValidator : AbstractValidator<Messages.RemoveRoadSegments>
{
    public RemoveRoadSegmentsValidator()
    {
        RuleFor(c => c.Ids)
            .NotEmpty();
        RuleForEach(c => c.Ids)
            .Must(RoadSegmentId.Accepts);

        RuleFor(c => c.GeometryDrawMethod)
            .NotEmpty()
            .WithProblemCode(ProblemCode.RoadSegment.GeometryDrawMethod.NotValid)
            .Must(RoadSegmentGeometryDrawMethod.CanParse)
            .WithProblemCode(ProblemCode.RoadSegment.GeometryDrawMethod.NotValid);
    }
}
