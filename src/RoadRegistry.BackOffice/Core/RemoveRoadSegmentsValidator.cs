namespace RoadRegistry.BackOffice.Core;

using CommandHandling;
using Extensions;
using FluentValidation;
using RoadRegistry.RoadNetwork.ValueObjects;
using RoadRegistry.RoadSegment.ValueObjects;
using ValueObjects.ProblemCodes;

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
