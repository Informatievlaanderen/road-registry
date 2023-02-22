namespace RoadRegistry.BackOffice.Handlers.RoadSegments;

using Abstractions.RoadSegments;
using Abstractions.Validation;
using Extensions;
using FluentValidation;

public class LinkStreetNameRequestValidator : AbstractValidator<LinkStreetNameRequest>
{
    public LinkStreetNameRequestValidator()
    {
        RuleFor(x => x.WegsegmentId)
            .GreaterThan(0)
            .WithErrorCode(ValidationErrors.Common.IncorrectObjectId.Code)
            .WithMessage(request => ValidationErrors.Common.IncorrectObjectId.Message(request.WegsegmentId));

        RuleFor(x => x.LinkerstraatnaamId)
            .MustBeValidStreetNamePuri()
            .WithErrorCode(ValidationErrors.Common.IncorrectObjectId.Code)
            .WithMessage(request => ValidationErrors.Common.IncorrectObjectId.Message(request.LinkerstraatnaamId));

        RuleFor(x => x.RechterstraatnaamId)
            .MustBeValidStreetNamePuri()
            .WithErrorCode(ValidationErrors.Common.IncorrectObjectId.Code)
            .WithMessage(request => ValidationErrors.Common.IncorrectObjectId.Message(request.RechterstraatnaamId));
    }
}
