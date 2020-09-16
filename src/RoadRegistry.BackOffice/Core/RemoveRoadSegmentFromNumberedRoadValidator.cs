namespace RoadRegistry.BackOffice.Core
{
    using FluentValidation;

    public class RemoveRoadSegmentFromNumberedRoadValidator : AbstractValidator<Messages.RemoveRoadSegmentFromNumberedRoad>
    {
        public RemoveRoadSegmentFromNumberedRoadValidator()
        {
            RuleFor(c => c.AttributeId).GreaterThanOrEqualTo(0);
            RuleFor(c => c.SegmentId).GreaterThanOrEqualTo(0);
            RuleFor(c => c.Ident8)
                .NotEmpty()
                .Must(NumberedRoadNumber.CanParse)
                .When(c => c.Ident8 != null, ApplyConditionTo.CurrentValidator);
        }
    }
}
