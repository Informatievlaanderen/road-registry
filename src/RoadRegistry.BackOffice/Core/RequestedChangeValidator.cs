namespace RoadRegistry.BackOffice.Core
{
    using FluentValidation;
    using Messages;

    public class RequestedChangeValidator : AbstractValidator<RequestedChange>
    {
        public RequestedChangeValidator()
        {
            RuleFor(c => c.AddRoadNode).SetValidator(new AddRoadNodeValidator());
            RuleFor(c => c.ModifyRoadNode).SetValidator(new ModifyRoadNodeValidator());
            RuleFor(c => c.AddRoadSegment).SetValidator(new AddRoadSegmentValidator());
            RuleFor(c => c.ModifyRoadSegment).SetValidator(new ModifyRoadSegmentValidator());
            RuleFor(c => c.AddRoadSegmentToEuropeanRoad).SetValidator(new AddRoadSegmentToEuropeanRoadValidator());
            RuleFor(c => c.AddRoadSegmentToNationalRoad).SetValidator(new AddRoadSegmentToNationalRoadValidator());
            RuleFor(c => c.AddRoadSegmentToNumberedRoad).SetValidator(new AddRoadSegmentToNumberedRoadValidator());
            RuleFor(c => c.AddGradeSeparatedJunction).SetValidator(new AddGradeSeparatedJunctionValidator());
        }
    }
}
