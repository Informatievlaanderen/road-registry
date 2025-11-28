namespace RoadRegistry.BackOffice.Core;

using FluentValidation;
using RoadRegistry.RoadNetwork.ValueObjects;

public class AddRoadNodeValidator : AbstractValidator<Messages.AddRoadNode>
{
    public AddRoadNodeValidator()
    {
        RuleFor(c => c.TemporaryId).GreaterThanOrEqualTo(1);
        RuleFor(c => c.Type)
            .NotEmpty()
            .Must(RoadNodeType.CanParse)
            .When(c => c.Type != null, ApplyConditionTo.CurrentValidator)
            .WithMessage("The 'Type' is not a RoadNodeType.");
        RuleFor(c => c.Geometry).NotNull().SetValidator(new RoadNodeGeometryValidator());
    }
}
