namespace RoadRegistry.BackOffice.Model
{
    using FluentValidation;
    using Messages;

    public class RequestedRoadSegmentSurfaceAttributeValidator : AbstractValidator<RequestedRoadSegmentSurfaceAttribute>
    {
        public RequestedRoadSegmentSurfaceAttributeValidator()
        {
            RuleFor(c => c.AttributeId).GreaterThanOrEqualTo(0);
            RuleFor(c => c.FromPosition).GreaterThanOrEqualTo(0.0m);
            RuleFor(c => c.ToPosition).GreaterThan(_ => _.FromPosition);
            RuleFor(c => c.Type)
                .NotEmpty()
                .Must(RoadSegmentSurfaceType.CanParse)
                .When(c => c.Type != null, ApplyConditionTo.CurrentValidator)
                .WithMessage("The 'Type' is not a RoadSegmentSurfaceType.");
        }
    }
}
