namespace RoadRegistry.BackOffice.Core
{
    public interface IRequestedChange
    {
        IVerifiedChange Verify(VerificationContext context);

        void TranslateTo(Messages.AcceptedChange message);
        void TranslateTo(Messages.RejectedChange message);
    }
}
