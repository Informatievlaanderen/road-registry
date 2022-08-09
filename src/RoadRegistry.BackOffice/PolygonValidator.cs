namespace RoadRegistry.BackOffice;

using FluentValidation;
using Messages;

public class PolygonValidator : AbstractValidator<Polygon>
{
    public PolygonValidator()
    {
        RuleFor(c => c.Shell).NotNull().SetValidator(new RingValidator());
        RuleFor(c => c.Holes).NotNull();
        RuleForEach(c => c.Holes).NotNull().SetValidator(new RingValidator());
    }
}
