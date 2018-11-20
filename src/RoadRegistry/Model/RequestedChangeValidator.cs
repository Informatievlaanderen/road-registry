namespace RoadRegistry.Model
{
    using FluentValidation;
    using Messages;

    public class RequestedChangeValidator : AbstractValidator<RequestedChange>
    {
        public RequestedChangeValidator()
        {
            RuleFor(c => c.AddRoadNode).SetValidator(new AddRoadNodeValidator());
            RuleFor(c => c.AddRoadSegment).SetValidator(new AddRoadSegmentValidator());
        }
    }
}
