namespace RoadRegistry.BackOffice.Core
{
    public interface IRequestedChange
    {
        Problems VerifyBefore(BeforeVerificationContext context);
        Problems VerifyAfter(AfterVerificationContext context);

        void TranslateTo(Messages.AcceptedChange message);
        void TranslateTo(Messages.RejectedChange message);
    }
}
