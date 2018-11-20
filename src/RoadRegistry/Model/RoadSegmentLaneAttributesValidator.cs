namespace RoadRegistry.Model
{
    using FluentValidation;
    using Messages;

    public class RoadSegmentLaneAttributesValidator : AbstractValidator<RequestedRoadSegmentLaneAttributes>
    {
        public const int Unknown = -8;
        public const int NotApplicable = -9;

        public RoadSegmentLaneAttributesValidator()
        {
            RuleFor(c => c.FromPosition).GreaterThanOrEqualTo(0.0m);
            RuleFor(c => c.ToPosition).GreaterThan(_ => _.FromPosition);
            RuleFor(c => c.Count)
                .InclusiveBetween(0, RoadSegmentLaneCount.Maximum.ToInt32())
                .When(_ => _.Count != Unknown && _.Count != NotApplicable, ApplyConditionTo.CurrentValidator);
            RuleFor(c => c.Direction)
                .NotEmpty()
                .Must(RoadSegmentLaneDirection.CanParse)
                .When(c => c.Direction != null, ApplyConditionTo.CurrentValidator)
                .WithMessage("The 'Direction' is not a RoadSegmentLaneDirection.");
        }
    }
}
