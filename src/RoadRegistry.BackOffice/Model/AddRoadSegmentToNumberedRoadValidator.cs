namespace RoadRegistry.BackOffice.Model
{
    using FluentValidation;

    public class AddRoadSegmentToNumberedRoadValidator : AbstractValidator<Messages.AddRoadSegmentToNumberedRoad>
    {
        public AddRoadSegmentToNumberedRoadValidator()
        {
            RuleFor(c => c.TemporaryAttributeId).GreaterThanOrEqualTo(0);
            RuleFor(c => c.SegmentId).GreaterThanOrEqualTo(0);
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
