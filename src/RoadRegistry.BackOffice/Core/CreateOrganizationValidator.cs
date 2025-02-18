namespace RoadRegistry.BackOffice.Core;

using FluentValidation;
using Messages;

public class CreateOrganizationValidator : AbstractValidator<CreateOrganization>
{
    public CreateOrganizationValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty();

        When(x => x.OvoCode is not null, () =>
        {
            RuleFor(x => x.OvoCode)
                .Must(OrganizationOvoCode.AcceptsValue);
        });

        When(x => x.KboNumber is not null, () =>
        {
            RuleFor(x => x.KboNumber)
                .Must(OrganizationKboNumber.AcceptsValue);
        });
    }
}
