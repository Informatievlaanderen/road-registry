namespace RoadRegistry.BackOffice.Handlers.RoadSegments;

using Abstractions.RoadSegments;
using BackOffice.Extensions;
using Core;
using Core.ProblemCodes;
using FluentValidation;

public class RoadSegmentDetailRequestValidator : AbstractValidator<RoadSegmentDetailRequest>
{
    public RoadSegmentDetailRequestValidator()
    {
        RuleFor(x => x.WegsegmentId)
            .GreaterThan(0)
            .WithProblemCode(ProblemCode.Common.IncorrectObjectId)
            ;
    }
}
