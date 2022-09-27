namespace RoadRegistry.BackOffice.Core;

using FluentValidation;

public class RemoveGradeSeparatedJunctionValidator : AbstractValidator<Messages.RemoveGradeSeparatedJunction>
{
    public RemoveGradeSeparatedJunctionValidator()
    {
        RuleFor(c => c.Id).GreaterThanOrEqualTo(0);
    }
}
