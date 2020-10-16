namespace RoadRegistry.BackOffice.Core
{
    using FluentValidation;

    public class AddRoadSegmentToNationalRoadValidator : AbstractValidator<Messages.AddRoadSegmentToNationalRoad>
    {
        public AddRoadSegmentToNationalRoadValidator()
        {
            RuleFor(c => c.TemporaryAttributeId).GreaterThanOrEqualTo(0);
            RuleFor(c => c.SegmentId).GreaterThanOrEqualTo(0);
            RuleFor(c => c.Number)
                .NotEmpty()
                .Must(NationalRoadNumber.CanParse)
                .When(c => c.Number != null, ApplyConditionTo.CurrentValidator);
        }
    }
}
