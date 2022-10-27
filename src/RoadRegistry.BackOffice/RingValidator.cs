namespace RoadRegistry.BackOffice;

using FluentValidation;
using Messages;

public class RingValidator : AbstractValidator<Ring>
{
    public RingValidator()
    {
        RuleFor(c => c.Points).NotNull();
        RuleForEach(c => c.Points).NotNull();
    }
}