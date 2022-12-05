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
    }
}
