namespace RoadRegistry.BackOffice.Core;

using FluentValidation;

public class ModifyRoadNodeValidator : AbstractValidator<Messages.ModifyRoadNode>
{
    public ModifyRoadNodeValidator()
    {
        RuleFor(c => c.Id).GreaterThanOrEqualTo(1);

        When(x => x.Type is not null, () =>
        {
            RuleFor(c => c.Type)
                .NotEmpty()
                .Must(RoadNodeType.CanParse)
                .When(c => c.Type != null, ApplyConditionTo.CurrentValidator)
                .WithMessage("The 'Type' is not a RoadNodeType.");
        });

        When(x => x.Geometry is not null, () =>
        {
            RuleFor(c => c.Geometry)
                .SetValidator(new RoadNodeGeometryValidator());
        });
    }
}
