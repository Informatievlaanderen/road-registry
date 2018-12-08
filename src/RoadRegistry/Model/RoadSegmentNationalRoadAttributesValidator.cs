namespace RoadRegistry.Model
{
    using FluentValidation;
    using Messages;

    public class RoadSegmentNationalRoadAttributesValidator : AbstractValidator<RoadSegmentNationalRoadAttributes>
    {
        public RoadSegmentNationalRoadAttributesValidator()
        {
            RuleFor(c => c.AttributeId).GreaterThanOrEqualTo(0);
            RuleFor(c => c.Ident2)
                .NotEmpty()
                .Must(NationalRoadNumber.CanParse)
                .When(c => c.Ident2 != null, ApplyConditionTo.CurrentValidator);
        }
    }
}
