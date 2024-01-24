namespace RoadRegistry.BackOffice.Core;

using FluentValidation;

public class RemoveOutlinedRoadSegmentFromRoadNetworkValidator : AbstractValidator<Messages.RemoveOutlinedRoadSegmentFromRoadNetwork>
{
    public RemoveOutlinedRoadSegmentFromRoadNetworkValidator()
    {
        RuleFor(c => c.Id).GreaterThanOrEqualTo(0);
    }
}
