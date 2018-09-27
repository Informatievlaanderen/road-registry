namespace RoadRegistry.Model
{
    using FluentValidation;

    public class AddRoadSegmentValidator : AbstractValidator<Commands.AddRoadSegment>
    {
        public AddRoadSegmentValidator()
        {
            RuleFor(c => c.Id).NotEmpty();
            RuleFor(c => c.Geometry).NotNull();
        }
    }
}
