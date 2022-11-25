namespace RoadRegistry.BackOffice.Core;

using FluentValidation;
using Messages;

public class RenameOrganizationValidator : AbstractValidator<RenameOrganization>
{
    public RenameOrganizationValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty();
        RuleFor(x => x.Name)
            .NotEmpty();
    }
}
