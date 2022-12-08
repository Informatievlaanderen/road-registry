namespace RoadRegistry.BackOffice.Core;

using FluentValidation;
using Messages;

public class DeleteOrganizationValidator : AbstractValidator<DeleteOrganization>
{
    public DeleteOrganizationValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty();
    }
}
