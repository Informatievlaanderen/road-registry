namespace RoadRegistry.BackOffice.Handlers.RoadSegments;

using Abstractions.RoadSegments;
using BackOffice.Extensions;
using Core;
using Core.ProblemCodes;
using Extensions;
using FluentValidation;
using FluentValidation.Results;

public class UnlinkStreetNameRequestValidator : AbstractValidator<UnlinkStreetNameRequest>
{
    protected override bool PreValidate(ValidationContext<UnlinkStreetNameRequest> context, ValidationResult result)
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

    public UnlinkStreetNameRequestValidator()
    {
        RuleFor(x => x.WegsegmentId)
            .Must(RoadSegmentId.IsValid)
            .WithProblemCode(ProblemCode.Common.IncorrectObjectId);

        RuleFor(x => x.LinkerstraatnaamId)
            .MustBeValidStreetNamePuri()
            .WithProblemCode(ProblemCode.Common.IncorrectObjectId);

        RuleFor(x => x.RechterstraatnaamId)
            .MustBeValidStreetNamePuri()
            .WithProblemCode(ProblemCode.Common.IncorrectObjectId);
    }
}
