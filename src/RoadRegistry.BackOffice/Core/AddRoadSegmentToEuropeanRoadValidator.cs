namespace RoadRegistry.BackOffice.Core;

using FluentValidation;
using RoadRegistry.RoadNetwork.ValueObjects;

public class AddRoadSegmentToEuropeanRoadValidator : AbstractValidator<Messages.AddRoadSegmentToEuropeanRoad>
{
    public AddRoadSegmentToEuropeanRoadValidator()
    {
        RuleFor(c => c.TemporaryAttributeId).GreaterThanOrEqualTo(0);
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
