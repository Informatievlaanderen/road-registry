namespace RoadRegistry.BackOffice.Core;

using FluentValidation;
using Messages;

public class RequestedChangeValidator : AbstractValidator<RequestedChange>
{
    public RequestedChangeValidator()
    {
        RuleFor(c => c.AddRoadNode).SetValidator(new AddRoadNodeValidator());
        RuleFor(c => c.ModifyRoadNode).SetValidator(new ModifyRoadNodeValidator());
        RuleFor(c => c.RemoveRoadNode).SetValidator(new RemoveRoadNodeValidator());
        RuleFor(c => c.AddRoadSegment).SetValidator(new AddRoadSegmentValidator());
        RuleFor(c => c.ModifyRoadSegment).SetValidator(new ModifyRoadSegmentValidator());
        RuleFor(c => c.RemoveRoadSegment).SetValidator(new RemoveRoadSegmentValidator());
        RuleFor(c => c.RemoveRoadSegments).SetValidator(new RemoveRoadSegmentsValidator());
        RuleFor(c => c.RemoveOutlinedRoadSegment).SetValidator(new RemoveOutlinedRoadSegmentValidator());
        RuleFor(c => c.RemoveOutlinedRoadSegmentFromRoadNetwork).SetValidator(new RemoveOutlinedRoadSegmentFromRoadNetworkValidator());
        RuleFor(c => c.AddRoadSegmentToEuropeanRoad).SetValidator(new AddRoadSegmentToEuropeanRoadValidator());
        RuleFor(c => c.RemoveRoadSegmentFromEuropeanRoad).SetValidator(new RemoveRoadSegmentFromEuropeanRoadValidator());
        RuleFor(c => c.AddRoadSegmentToNationalRoad).SetValidator(new AddRoadSegmentToNationalRoadValidator());
        RuleFor(c => c.RemoveRoadSegmentFromNationalRoad).SetValidator(new RemoveRoadSegmentFromNationalRoadValidator());
        RuleFor(c => c.AddRoadSegmentToNumberedRoad).SetValidator(new AddRoadSegmentToNumberedRoadValidator());
        RuleFor(c => c.RemoveRoadSegmentFromNumberedRoad).SetValidator(new RemoveRoadSegmentFromNumberedRoadValidator());
        RuleFor(c => c.AddGradeSeparatedJunction).SetValidator(new AddGradeSeparatedJunctionValidator());
        RuleFor(c => c.ModifyGradeSeparatedJunction).SetValidator(new ModifyGradeSeparatedJunctionValidator());
        RuleFor(c => c.RemoveGradeSeparatedJunction).SetValidator(new RemoveGradeSeparatedJunctionValidator());
    }
}
