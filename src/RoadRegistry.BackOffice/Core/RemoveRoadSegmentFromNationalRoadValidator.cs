namespace RoadRegistry.BackOffice.Core
{
    using FluentValidation;

    public class RemoveRoadSegmentFromNationalRoadValidator : AbstractValidator<Messages.RemoveRoadSegmentFromNationalRoad>
    {
        public RemoveRoadSegmentFromNationalRoadValidator()
        {
            RuleFor(c => c.AttributeId).GreaterThanOrEqualTo(0);
            RuleFor(c => c.SegmentId).GreaterThanOrEqualTo(0);
            RuleFor(c => c.Number)
                .NotEmpty()
                .Must(NationalRoadNumber.CanParse)
                .When(c => c.Number != null, ApplyConditionTo.CurrentValidator);
        }
    }
}
