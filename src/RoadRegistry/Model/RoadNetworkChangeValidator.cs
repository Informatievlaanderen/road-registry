namespace RoadRegistry.Model
{
    using FluentValidation;

    public class RoadNetworkChangeValidator : AbstractValidator<Commands.RoadNetworkChange>
    {
        public RoadNetworkChangeValidator()
        {
            RuleFor(c => c.AddRoadNode).NotNull().SetValidator(new AddRoadNodeValidator());
        }
    }
}