namespace RoadRegistry.Model
{
    using FluentValidation;
    using Messages;

    public class RoadSegmentNumberedRoadPropertiesValidator : AbstractValidator<RequestedRoadSegmentNumberedRoadProperties>
    {
        public RoadSegmentNumberedRoadPropertiesValidator()
        {
            RuleFor(c => c.Ident8)
                .NotEmpty()
                .Must(NumberedRoadNumber.CanParse)
                .When(c => c.Ident8 != null, ApplyConditionTo.CurrentValidator);
            RuleFor(c => c.Direction).IsInEnum();
            RuleFor(c => c.Ordinal).GreaterThanOrEqualTo(0);
        }
    }
}
