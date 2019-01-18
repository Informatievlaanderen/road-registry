namespace RoadRegistry.BackOffice.Model
{
    using FluentValidation;
    using Messages;

    public class RequestedChangeValidator : AbstractValidator<RequestedChange>
    {
        public RequestedChangeValidator()
        {
            RuleFor(c => c.AddRoadNode).SetValidator(new AddRoadNodeValidator());
            RuleFor(c => c.AddRoadSegment).SetValidator(new AddRoadSegmentValidator());
            RuleFor(c => c.AddRoadSegmentToEuropeanRoad).SetValidator(new AddRoadSegmentToEuropeanRoadValidator());
            RuleFor(c => c.AddRoadSegmentToNationalRoad).SetValidator(new AddRoadSegmentToNationalRoadValidator());
            RuleFor(c => c.AddRoadSegmentToNumberedRoad).SetValidator(new AddRoadSegmentToNumberedRoadValidator());
            RuleFor(c => c.AddGradeSeparatedJunction).SetValidator(new AddGradeSeparatedJunctionValidator());
        }
    }
}
