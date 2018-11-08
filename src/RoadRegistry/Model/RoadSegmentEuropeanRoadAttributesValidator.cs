namespace RoadRegistry.Model
{
    using FluentValidation;
    using Messages;

    public class RoadSegmentEuropeanRoadAttributesValidator : AbstractValidator<RequestedRoadSegmentEuropeanRoadAttributes>
    {
        public RoadSegmentEuropeanRoadAttributesValidator()
        {
            RuleFor(c => c.RoadNumber)
                .NotEmpty()
                .Must(EuropeanRoadNumber.CanParse)
                .When(c => c.RoadNumber != null, ApplyConditionTo.CurrentValidator);
        }
    }
}
