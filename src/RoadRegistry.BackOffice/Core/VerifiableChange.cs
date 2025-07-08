namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Linq;

public class VerifiableChange
{
    private readonly Problems _problems;
    private readonly IRequestedChange _requestedChange;

    public VerifiableChange(IRequestedChange change)
    {
        _requestedChange = change ?? throw new ArgumentNullException(nameof(change));
        _problems = Problems.None;
    }

    private VerifiableChange(IRequestedChange change, Problems problems)
    {
        _requestedChange = change;
        _problems = problems;
    }

    public bool HasErrors => _problems.OfType<Error>().Any();
    public bool HasWarnings => _problems.OfType<Warning>().Any();

    public IVerifiedChange AsVerifiedChange()
    {
        IVerifiedChange change;
        if (HasErrors)
            change = new RejectedChange(_requestedChange, _problems);
        else
            change = new AcceptedChange(_requestedChange, _problems);
        return change;
    }

    public (VerifiableChange, VerifyAfterResult) VerifyAfter(AfterVerificationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var result = _requestedChange.VerifyAfter(context);

        return (
            new VerifiableChange(
                _requestedChange,
                _problems.AddRange(result.Problems)),
            result);
    }

    public VerifiableChange VerifyBefore(BeforeVerificationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return new VerifiableChange(
            _requestedChange,
            _problems.AddRange(_requestedChange.VerifyBefore(context)));
    }
}
