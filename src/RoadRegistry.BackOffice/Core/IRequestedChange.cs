namespace RoadRegistry.BackOffice.Core;

using System.Collections.Generic;

public interface IRequestedChange
{
    IEnumerable<Messages.AcceptedChange> TranslateTo(Messages.Problem[] warnings);
    void TranslateTo(Messages.RejectedChange message);
    Problems VerifyAfter(AfterVerificationContext context);
    Problems VerifyBefore(BeforeVerificationContext context);
}
