namespace RoadRegistry.BackOffice.Core;

using System.Linq;
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

public class RequestedRoadSegmentOutlineSurfaceAttributeValidator : RequestedRoadSegmentSurfaceAttributeValidator
{
    public RequestedRoadSegmentOutlineSurfaceAttributeValidator()
    {
        var invalidRoadSegmentSurfaceTypes = new[] { RoadSegmentSurfaceType.Unknown, RoadSegmentSurfaceType.NotApplicable };

        RuleFor(c => c.Type)
            .NotEmpty()
            .Must(value => RoadSegmentSurfaceType.CanParse(value) && !invalidRoadSegmentSurfaceTypes.Contains(RoadSegmentSurfaceType.Parse(value)))
            .When(c => c.Type != null, ApplyConditionTo.CurrentValidator)
            .WithMessage("The 'Type' is not a valid RoadSegmentSurfaceType.");
    }
}
