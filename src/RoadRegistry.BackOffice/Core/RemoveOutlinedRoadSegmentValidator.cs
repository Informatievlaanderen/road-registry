namespace RoadRegistry.BackOffice.Core;

using FluentValidation;

public class RemoveOutlinedRoadSegmentValidator : AbstractValidator<Messages.RemoveOutlinedRoadSegment>
{
    public RemoveOutlinedRoadSegmentValidator()
    {
        RuleFor(c => c.Id).GreaterThanOrEqualTo(0);
    }
}
