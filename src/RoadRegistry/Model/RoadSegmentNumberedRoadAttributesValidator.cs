namespace RoadRegistry.Model
{
    using FluentValidation;
    using Messages;

    public class RoadSegmentNumberedRoadAttributesValidator : AbstractValidator<RequestedRoadSegmentNumberedRoadAttributes>
    {
        public RoadSegmentNumberedRoadAttributesValidator()
        {
            RuleFor(c => c.Ident8)
                .NotEmpty()
                .Must(NumberedRoadNumber.CanParse)
                .When(c => c.Ident8 != null, ApplyConditionTo.CurrentValidator);
            RuleFor(c => c.Direction)
                .NotEmpty()
                .Must(RoadSegmentNumberedRoadDirection.CanParse)
                .When(c => c.Direction != null, ApplyConditionTo.CurrentValidator)
                .WithMessage("The 'Direction' is not a RoadSegmentNumberedRoadDirection.");
            RuleFor(c => c.Ordinal).GreaterThanOrEqualTo(0);
        }
    }
}
