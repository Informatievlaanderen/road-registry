namespace RoadRegistry.Model
{
    using FluentValidation;
    using RoadRegistry.Commands;

    public class RoadSegmentNumberedRoadPropertiesValidator : AbstractValidator<Shared.RoadSegmentNumberedRoadProperties>
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
