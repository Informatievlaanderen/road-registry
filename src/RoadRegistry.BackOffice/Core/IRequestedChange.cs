namespace RoadRegistry.BackOffice.Core;

public interface IRequestedChange
{
    void TranslateTo(Messages.AcceptedChange message);
    void TranslateTo(Messages.RejectedChange message);
    Problems VerifyAfter(AfterVerificationContext context);
    Problems VerifyBefore(BeforeVerificationContext context);
}