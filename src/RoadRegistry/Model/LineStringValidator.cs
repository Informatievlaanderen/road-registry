namespace RoadRegistry.Model
{
    using FluentValidation;

    public class LineStringValidator : AbstractValidator<Messages.LineString>
    {
        public LineStringValidator()
        {
            RuleFor(c => c.Points).NotNull();
            RuleForEach(c => c.Points).NotNull();
        }
    }
}