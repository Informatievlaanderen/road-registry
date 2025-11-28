namespace RoadRegistry.BackOffice.Handlers.RoadSegments;

using Abstractions.RoadSegments;
using BackOffice.Extensions;
using CommandHandling;
using Core;
using Extensions;
using FluentValidation;
using FluentValidation.Results;
using ValueObjects.ProblemCodes;

public class LinkStreetNameRequestValidator : AbstractValidator<LinkStreetNameRequest>
{
    protected override bool PreValidate(ValidationContext<LinkStreetNameRequest> context, ValidationResult result)
    {
        if (context.InstanceToValidate.LinkerstraatnaamId.GetIdentifierFromPuri() <= 0
            && context.InstanceToValidate.RechterstraatnaamId.GetIdentifierFromPuri() <= 0)
        {
            context.AddFailure(new ValidationFailure
            {
                PropertyName = "request",
                ErrorCode = ProblemCode.Common.JsonInvalid
            });

            return false;
        }

        return base.PreValidate(context, result);
    }

    public LinkStreetNameRequestValidator()
    {
        RuleFor(x => x.WegsegmentId)
            .Must(RoadSegmentId.Accepts)
            .WithProblemCode(ProblemCode.Common.IncorrectObjectId);

        RuleFor(x => x.LinkerstraatnaamId)
            .MustBeValidStreetNameId()
            .WithProblemCode(ProblemCode.Common.IncorrectObjectId);

        RuleFor(x => x.RechterstraatnaamId)
            .MustBeValidStreetNameId()
            .WithProblemCode(ProblemCode.Common.IncorrectObjectId);
    }
}
