namespace RoadRegistry.BackOffice.Core;

using FluentValidation;

public class AddGradeSeparatedJunctionValidator : AbstractValidator<Messages.AddGradeSeparatedJunction>
{
    public AddGradeSeparatedJunctionValidator()
    {
        RuleFor(c => c.TemporaryId).GreaterThanOrEqualTo(0);
        RuleFor(c => c.UpperSegmentId).GreaterThanOrEqualTo(0);
        RuleFor(c => c.LowerSegmentId).GreaterThanOrEqualTo(0);
        RuleFor(c => c.Type)
            .NotEmpty()
            .Must(GradeSeparatedJunctionType.CanParse)
            .When(c => c.Type != null, ApplyConditionTo.CurrentValidator)
            .WithMessage("The 'Type' is not a GradeSeparatedJunctionType.");
    }
}
