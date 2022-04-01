namespace RoadRegistry.BackOffice.Validators.AddGradeSeparatedJunction.After
{
    using Core;

    internal class AddGradeSeparatedJunctionWithAfterVerificationContext
    {
        public AddGradeSeparatedJunction AddGradeSeparatedJunction { get; }
        public AfterVerificationContext AfterVerificationContext { get; }

        public AddGradeSeparatedJunctionWithAfterVerificationContext(AddGradeSeparatedJunction addGradeSeparatedJunction, AfterVerificationContext afterVerificationContext)
        {
            AddGradeSeparatedJunction = addGradeSeparatedJunction;
            AfterVerificationContext = afterVerificationContext;
        }
    }
}
