namespace RoadRegistry.Model
{
    using Commands;
    using FluentValidation;

    public class AddRoadNodeValidator : AbstractValidator<Commands.AddRoadNode>
    {
        public AddRoadNodeValidator()
        {
            RuleFor(c => c.Id).NotEmpty();
            RuleFor(c => c.Geometry).NotNull();
        }
    }
}
