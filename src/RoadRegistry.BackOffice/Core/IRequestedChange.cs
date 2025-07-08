namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Linq;

public interface IRequestedChange
{
    //IEnumerable<Messages.AcceptedChange> TranslateTo(Messages.Problem[] warnings);
    Problems VerifyBefore(BeforeVerificationContext context);
    VerifyAfterResult VerifyAfter(AfterVerificationContext context);
    void TranslateToRejectedChange(Messages.RejectedChange message);
}

public sealed record VerifyAfterResult
{
    public VerifyAfterResult(Problems problems)
        : this(problems, [])
    {
    }

    public VerifyAfterResult(Problems problems, IEnumerable<Messages.AcceptedChange> acceptedChanges)
    {
        Problems = problems;
        AcceptedChanges = acceptedChanges.ToList();
    }

    public Problems Problems { get; }
    public List<Messages.AcceptedChange> AcceptedChanges { get; }

    public static VerifyAfterResult WithAcceptedChanges(Problems problems, Func<Messages.Problem[], IEnumerable<Messages.AcceptedChange>> getAcceptedChanges)
    {
        if (problems.OfType<Error>().Any())
        {
            return new VerifyAfterResult(problems);
        }

        var warnings = problems.Select(x => x.Translate()).ToArray();
        return new VerifyAfterResult(problems, getAcceptedChanges(warnings));
    }
}
