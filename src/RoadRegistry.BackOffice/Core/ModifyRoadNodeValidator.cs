namespace RoadRegistry.BackOffice.Core
{
    using FluentValidation;

    public class ModifyRoadNodeValidator : AbstractValidator<Messages.ModifyRoadNode>
    {
        public ModifyRoadNodeValidator()
        {
            RuleFor(c => c.Id).GreaterThanOrEqualTo(0);
            RuleFor(c => c.Type)
                .NotEmpty()
                .Must(RoadNodeType.CanParse)
                .When(c => c.Type != null, ApplyConditionTo.CurrentValidator)
                .WithMessage("The 'Type' is not a RoadNodeType.");
            RuleFor(c => c.Geometry).NotNull().SetValidator(new RoadNodeGeometryValidator());
        }
    }
}
