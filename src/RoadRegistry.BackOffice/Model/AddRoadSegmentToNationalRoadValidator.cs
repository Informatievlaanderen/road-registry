namespace RoadRegistry.BackOffice.Model
{
    using FluentValidation;

    public class AddRoadSegmentToNationalRoadValidator : AbstractValidator<Messages.AddRoadSegmentToNationalRoad>
    {
        public AddRoadSegmentToNationalRoadValidator()
        {
            RuleFor(c => c.TemporaryAttributeId).GreaterThanOrEqualTo(0);
            RuleFor(c => c.SegmentId).GreaterThanOrEqualTo(0);
            RuleFor(c => c.Ident2)
                .NotEmpty()
                .Must(NationalRoadNumber.CanParse)
                .When(c => c.Ident2 != null, ApplyConditionTo.CurrentValidator);
        }
    }
}
