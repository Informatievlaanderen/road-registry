namespace RoadRegistry.BackOffice
{
    using FluentValidation;

    public class RingValidator : AbstractValidator<Messages.Ring>
    {
        public RingValidator()
        {
            RuleFor(c => c.Points).NotNull();
            RuleForEach(c => c.Points).NotNull();
        }
    }
}