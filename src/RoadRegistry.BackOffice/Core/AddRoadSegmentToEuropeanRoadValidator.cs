namespace RoadRegistry.BackOffice.Core;

using FluentValidation;

public class AddRoadSegmentToEuropeanRoadValidator : AbstractValidator<Messages.AddRoadSegmentToEuropeanRoad>
{
    public AddRoadSegmentToEuropeanRoadValidator()
    {
        RuleFor(c => c.TemporaryAttributeId).GreaterThanOrEqualTo(0);
        RuleFor(c => c.SegmentId).GreaterThanOrEqualTo(0);
        RuleFor(c => c.Number)
            .NotEmpty()
            .Must(EuropeanRoadNumber.CanParse)
            .When(c => c.Number != null, ApplyConditionTo.CurrentValidator);
    }
}