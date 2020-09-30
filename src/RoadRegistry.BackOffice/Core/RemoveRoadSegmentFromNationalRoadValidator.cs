namespace RoadRegistry.BackOffice.Core
{
    using FluentValidation;

    public class RemoveRoadSegmentFromNationalRoadValidator : AbstractValidator<Messages.RemoveRoadSegmentFromNationalRoad>
    {
        public RemoveRoadSegmentFromNationalRoadValidator()
        {
            RuleFor(c => c.AttributeId).GreaterThanOrEqualTo(0);
            RuleFor(c => c.SegmentId).GreaterThanOrEqualTo(0);
            RuleFor(c => c.Ident2)
                .NotEmpty()
                .Must(NationalRoadNumber.CanParse)
                .When(c => c.Ident2 != null, ApplyConditionTo.CurrentValidator);
        }
    }
}
