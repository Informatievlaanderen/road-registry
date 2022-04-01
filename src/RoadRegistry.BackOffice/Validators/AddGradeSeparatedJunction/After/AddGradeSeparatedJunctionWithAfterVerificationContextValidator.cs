namespace RoadRegistry.BackOffice.Validators.AddGradeSeparatedJunction.After
{
    using FluentValidation;

    internal class AddGradeSeparatedJunctionWithAfterVerificationContextValidator : AbstractValidator<AddGradeSeparatedJunctionWithAfterVerificationContext>
    {
        public AddGradeSeparatedJunctionWithAfterVerificationContextValidator()
        {
            RuleFor(x => x.AddGradeSeparatedJunction)
                .SetValidator(new SegmentsValidator());
        }
    }
}
