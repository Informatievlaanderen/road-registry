namespace RoadRegistry.BackOffice.Core;

using FluentValidation;

public class RemoveRoadSegmentFromEuropeanRoadValidator : AbstractValidator<Messages.RemoveRoadSegmentFromEuropeanRoad>
{
    public RemoveRoadSegmentFromEuropeanRoadValidator()
    {
        RuleFor(c => c.AttributeId).GreaterThanOrEqualTo(0);
        RuleFor(c => c.SegmentId).GreaterThanOrEqualTo(0);
        RuleFor(c => c.Number)
            .NotEmpty()
            .Must(EuropeanRoadNumber.CanParse)
            .When(c => c.Number != null, ApplyConditionTo.CurrentValidator);
    }
}