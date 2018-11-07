namespace RoadRegistry.Model
{
    using FluentValidation;
    using Messages;

    public class RoadSegmentEuropeanRoadPropertiesValidator : AbstractValidator<RequestedRoadSegmentEuropeanRoadProperties>
    {
        public RoadSegmentEuropeanRoadPropertiesValidator()
        {
            RuleFor(c => c.RoadNumber)
                .NotEmpty()
                .Must(EuropeanRoadNumber.CanParse)
                .When(c => c.RoadNumber != null, ApplyConditionTo.CurrentValidator);
        }
    }
}
