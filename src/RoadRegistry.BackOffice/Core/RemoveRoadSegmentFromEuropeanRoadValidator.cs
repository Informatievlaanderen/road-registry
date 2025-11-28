namespace RoadRegistry.BackOffice.Core;

using FluentValidation;
using RoadRegistry.RoadNetwork.ValueObjects;

public class RemoveRoadSegmentFromEuropeanRoadValidator : AbstractValidator<Messages.RemoveRoadSegmentFromEuropeanRoad>
{
    public RemoveRoadSegmentFromEuropeanRoadValidator()
    {
        RuleFor(c => c.AttributeId).GreaterThanOrEqualTo(0);
        RuleFor(c => c.SegmentId).GreaterThanOrEqualTo(0);
        RuleFor(c => c.SegmentGeometryDrawMethod)
            .NotEmpty()
            .Must(RoadSegmentGeometryDrawMethod.CanParse);
        RuleFor(c => c.Number)
            .NotEmpty()
            .Must(EuropeanRoadNumber.CanParse)
            .When(c => c.Number != null, ApplyConditionTo.CurrentValidator);
    }
}
