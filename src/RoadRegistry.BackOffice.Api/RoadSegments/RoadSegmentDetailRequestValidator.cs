namespace RoadRegistry.BackOffice.Api.RoadSegments;

using Abstractions.RoadSegments;
using Core.ProblemCodes;
using Extensions;
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
