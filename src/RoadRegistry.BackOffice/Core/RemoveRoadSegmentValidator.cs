namespace RoadRegistry.BackOffice.Core;

using FluentValidation;

public class RemoveRoadSegmentValidator : AbstractValidator<Messages.RemoveRoadSegment>
{
    public RemoveRoadSegmentValidator()
    {
        RuleFor(c => c.Id).GreaterThanOrEqualTo(0);
    }
}