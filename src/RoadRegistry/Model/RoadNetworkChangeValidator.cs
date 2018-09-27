namespace RoadRegistry.Model
{
    using FluentValidation;

    public class RoadNetworkChangeValidator : AbstractValidator<Commands.RoadNetworkChange>
    {
        public RoadNetworkChangeValidator()
        {
            RuleFor(c => c.AddRoadNode).SetValidator(new AddRoadNodeValidator());
            RuleFor(c => c.AddRoadSegment).SetValidator(new AddRoadSegmentValidator());
        }
    }
}