namespace RoadRegistry.BackOffice.Core;

using FluentValidation;
using RoadRegistry.RoadNetwork.ValueObjects;

public class RemoveRoadSegmentFromNationalRoadValidator : AbstractValidator<Messages.RemoveRoadSegmentFromNationalRoad>
{
    public RemoveRoadSegmentFromNationalRoadValidator()
    {
        RuleFor(c => c.AttributeId).GreaterThanOrEqualTo(0);
        RuleFor(c => c.SegmentId).GreaterThanOrEqualTo(0);
        RuleFor(c => c.SegmentGeometryDrawMethod)
            .NotEmpty()
            .Must(RoadSegmentGeometryDrawMethod.CanParse);
        RuleFor(c => c.Number)
            .NotEmpty()
            .Must(NationalRoadNumber.CanParse)
            .When(c => c.Number != null, ApplyConditionTo.CurrentValidator);
    }
}
