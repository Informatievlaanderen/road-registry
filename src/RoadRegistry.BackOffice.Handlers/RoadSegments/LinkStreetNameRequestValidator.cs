namespace RoadRegistry.BackOffice.Handlers.RoadSegments;

using Abstractions.RoadSegments;
using Abstractions.Validation;
using Extensions;
using FluentValidation;
using FluentValidation.Results;

public class LinkStreetNameRequestValidator : AbstractValidator<LinkStreetNameRequest>
{
    protected override bool PreValidate(ValidationContext<LinkStreetNameRequest> context, ValidationResult result)
    {
        if (context.InstanceToValidate.LinkerstraatnaamId.GetIdentifierFromPuri() <= 0
            && context.InstanceToValidate.RechterstraatnaamId.GetIdentifierFromPuri() <= 0)
        {
            context.AddFailure(new ValidationFailure("request", ValidationErrors.Common.JsonInvalid.Message)
            {
                ErrorCode = ValidationErrors.Common.JsonInvalid.Code
            });

            return false;
        }

        return base.PreValidate(context, result);
    }

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
