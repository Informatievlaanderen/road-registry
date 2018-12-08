namespace RoadRegistry.Model
{
    using FluentValidation;
    using Messages;

    public class RoadSegmentEuropeanRoadAttributesValidator : AbstractValidator<RoadSegmentEuropeanRoadAttributes>
    {
        public RoadSegmentEuropeanRoadAttributesValidator()
        {
            RuleFor(c => c.AttributeId).GreaterThanOrEqualTo(0);
            RuleFor(c => c.RoadNumber)
                .NotEmpty()
                .Must(EuropeanRoadNumber.CanParse)
                .When(c => c.RoadNumber != null, ApplyConditionTo.CurrentValidator);
        }
    }
}
