namespace RoadRegistry.BackOffice.Handlers.RoadSegments;

using Abstractions.RoadSegments;
using BackOffice.Extensions;
using CommandHandling;
using Core;
using FluentValidation;
using RoadRegistry.Infrastructure;
using ValueObjects.ProblemCodes;

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
