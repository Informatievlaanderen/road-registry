namespace RoadRegistry.Model
{
    using FluentValidation;
    using Messages;

    public class RequestedRoadSegmentLaneAttributeValidator : AbstractValidator<RequestedRoadSegmentLaneAttribute>
    {
        public RequestedRoadSegmentLaneAttributeValidator()
        {
            RuleFor(c => c.AttributeId).GreaterThanOrEqualTo(0);
            RuleFor(c => c.FromPosition).GreaterThanOrEqualTo(0.0m);
            RuleFor(c => c.ToPosition).GreaterThan(_ => _.FromPosition);
            RuleFor(c => c.Count)
                .InclusiveBetween(0, RoadSegmentLaneCount.Maximum.ToInt32())
                .When(_ => _.Count != RoadSegmentLaneCount.Unknown && _.Count != RoadSegmentLaneCount.NotApplicable, ApplyConditionTo.CurrentValidator);
            RuleFor(c => c.Direction)
                .NotEmpty()
                .Must(RoadSegmentLaneDirection.CanParse)
                .When(c => c.Direction != null, ApplyConditionTo.CurrentValidator)
                .WithMessage("The 'Direction' is not a RoadSegmentLaneDirection.");
        }
    }
}
