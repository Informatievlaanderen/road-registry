namespace RoadRegistry.BackOffice;

using FluentValidation;
using Messages;

public class LineStringValidator : AbstractValidator<LineString>
{
    public LineStringValidator()
    {
        RuleFor(c => c.Points).NotNull();
        RuleForEach(c => c.Points).NotNull();
        RuleFor(c => c.Measures).NotNull();
        RuleForEach(c => c.Measures)
            .Must(value =>
                !double.IsNaN(value) && !double.IsNegativeInfinity(value) && !double.IsPositiveInfinity(value))
            .WithMessage("Each measure must not be NotANumber, Positive nor Negative Infinity.");
    }
}
