namespace RoadRegistry.Model
{
    using FluentValidation;
    using Messages;

    public class RoadSegmentNationalRoadAttributesValidator : AbstractValidator<RequestedRoadSegmentNationalRoadAttributes>
    {
        public RoadSegmentNationalRoadAttributesValidator()
        {
            RuleFor(c => c.Ident2)
                .NotEmpty()
                .Must(NationalRoadNumber.CanParse)
                .When(c => c.Ident2 != null, ApplyConditionTo.CurrentValidator);
        }
    }
}
