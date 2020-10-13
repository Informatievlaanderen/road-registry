namespace RoadRegistry.BackOffice.Core
{
    public interface IRequestedChange
    {
        Problems VerifyBefore(VerificationContext context);
        Problems VerifyAfter(VerificationContext context);

        void TranslateTo(Messages.AcceptedChange message);
        void TranslateTo(Messages.RejectedChange message);
    }
}
