namespace RoadRegistry.Model
{
    using FluentValidation;
    using RoadRegistry.Commands;

    public class RoadSegmentEuropeanRoadPropertiesValidator : AbstractValidator<Shared.RoadSegmentEuropeanRoadProperties>
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
