namespace RoadRegistry.BackOffice.Core;

using FluentValidation;
using Messages;

public class RenameOrganizationCommandValidator : AbstractValidator<RenameOrganization>
{
    public RenameOrganizationCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty();
        RuleFor(x => x.Name)
            .NotEmpty();
    }
}
