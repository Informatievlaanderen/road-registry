namespace RoadRegistry.BackOffice.Core;

using FluentValidation;

public class ModifyRoadSegmentOnNumberedRoadValidator : AbstractValidator<Messages.ModifyRoadSegmentOnNumberedRoad>
{
    public ModifyRoadSegmentOnNumberedRoadValidator()
    {
        RuleFor(c => c.AttributeId).GreaterThanOrEqualTo(0);
        RuleFor(c => c.SegmentId).GreaterThanOrEqualTo(0);
        RuleFor(c => c.Number)
            .NotEmpty()
            .Must(NumberedRoadNumber.CanParse)
            .When(c => c.Number != null, ApplyConditionTo.CurrentValidator);
        RuleFor(c => c.Direction)
            .NotEmpty()
            .Must(RoadSegmentNumberedRoadDirection.CanParse)
            .When(c => c.Direction != null, ApplyConditionTo.CurrentValidator)
            .WithMessage("The 'Direction' is not a RoadSegmentNumberedRoadDirection.");
        RuleFor(c => c.Ordinal).GreaterThanOrEqualTo(0);
    }
}