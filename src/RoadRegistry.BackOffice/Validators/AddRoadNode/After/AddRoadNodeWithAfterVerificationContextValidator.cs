namespace RoadRegistry.BackOffice.Validators.AddRoadNode.After
{
    using FluentValidation;

    internal class AddRoadNodeWithAfterVerificationContextValidator : AbstractValidator<AddRoadNodeWithAfterVerificationContext>
    {
        public AddRoadNodeWithAfterVerificationContextValidator()
        {
            RuleFor(x => x.AfterVerificationContext.AfterView.Nodes)
                .SetValidator(new GeometryTakenValidator());

            RuleFor(x => x.AddRoadNode)
                .SetValidator(new TypeMatchesConnectedValidator());
        }
    }
}
