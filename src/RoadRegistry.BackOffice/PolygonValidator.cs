namespace RoadRegistry.BackOffice
{
    using Extracts;
    using FluentValidation;

    public class PolygonValidator : AbstractValidator<Messages.Polygon>
    {
        public PolygonValidator()
        {
            RuleFor(c => c.Shell).NotNull().SetValidator(new RingValidator());
            RuleFor(c => c.Holes).NotNull();
            RuleForEach(c => c.Holes).NotNull().SetValidator(new RingValidator());
        }
    }
}