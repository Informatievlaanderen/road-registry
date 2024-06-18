namespace RoadRegistry.BackOffice.Core;

using FluentValidation;

public class RemoveRoadSegmentFromNumberedRoadValidator : AbstractValidator<Messages.RemoveRoadSegmentFromNumberedRoad>
{
    public RemoveRoadSegmentFromNumberedRoadValidator()
    {
        RuleFor(c => c.AttributeId).GreaterThanOrEqualTo(0);
        RuleFor(c => c.SegmentId).GreaterThanOrEqualTo(0);
        RuleFor(c => c.SegmentGeometryDrawMethod)
            .NotEmpty()
            .Must(RoadSegmentGeometryDrawMethod.CanParse);
        RuleFor(c => c.Number)
            .NotEmpty()
            .Must(NumberedRoadNumber.CanParse)
            .When(c => c.Number != null, ApplyConditionTo.CurrentValidator);
    }
}
