namespace RoadRegistry.BackOffice.Handlers.RoadSegments;

using Abstractions.RoadSegments;
using Abstractions.Validation;
using FluentValidation;

public class RoadSegmentDetailRequestValidator : AbstractValidator<RoadSegmentDetailRequest>
{
    public RoadSegmentDetailRequestValidator()
    {
        RuleFor(x => x.WegsegmentId)
            .GreaterThan(0)
            .WithErrorCode(ValidationErrors.Common.IncorrectObjectId.Code)
            .WithMessage(request => ValidationErrors.Common.IncorrectObjectId.Message(request.WegsegmentId));
    }
}
