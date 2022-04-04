namespace RoadRegistry.BackOffice.Validators.AddGradeSeparatedJunction.After
{
    using Core;
    using FluentValidation;

    internal class AddGradeSeparatedJunctionWithAfterVerificationContextValidator : AbstractValidator<(AddGradeSeparatedJunction, AfterVerificationContext)>
    {
        public AddGradeSeparatedJunctionWithAfterVerificationContextValidator()
        {
            RuleFor(x => x.Item1)
                .SetValidator(new SegmentsValidator());
        }
    }
}
