namespace RoadRegistry.Model
{
    using FluentValidation;
    using Messages;

    public class AddRoadSegmentToEuropeanRoadValidator : AbstractValidator<Messages.AddRoadSegmentToEuropeanRoad>
    {
        public AddRoadSegmentToEuropeanRoadValidator()
        {
            RuleFor(c => c.TemporaryAttributeId).GreaterThanOrEqualTo(0);
            RuleFor(c => c.SegmentId).GreaterThanOrEqualTo(0);
            RuleFor(c => c.RoadNumber)
                .NotEmpty()
                .Must(EuropeanRoadNumber.CanParse)
                .When(c => c.RoadNumber != null, ApplyConditionTo.CurrentValidator);
        }
    }
}
