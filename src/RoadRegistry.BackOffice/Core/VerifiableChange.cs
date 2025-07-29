namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Linq;

public class VerifiableChange
{
    private readonly IRequestedChange _requestedChange;
    private readonly Problems _problems;
    private readonly List<Messages.AcceptedChange> _acceptedChanges;

    public VerifiableChange(IRequestedChange change)
    {
        _requestedChange = change ?? throw new ArgumentNullException(nameof(change));
        _problems = Problems.None;
    }

    private VerifiableChange(IRequestedChange change, Problems problems, List<Messages.AcceptedChange> acceptedChanges)
    {
        _requestedChange = change;
        _problems = problems;
        _acceptedChanges = acceptedChanges;
    }

    public bool HasErrors => _problems.OfType<Error>().Any();

    public IVerifiedChange AsVerifiedChange()
    {
        IVerifiedChange change;
        if (HasErrors)
            change = new RejectedChange(_requestedChange, _problems);
        else
            change = new AcceptedChange(_acceptedChanges);
        return change;
    }

    public VerifiableChange VerifyAfter(AfterVerificationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var result = _requestedChange.VerifyAfter(context);

        return new VerifiableChange(
            _requestedChange,
            _problems.AddRange(result.Problems),
            result.AcceptedChanges);
    }

    public VerifiableChange VerifyBefore(BeforeVerificationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return new VerifiableChange(
            _requestedChange,
            _problems.AddRange(_requestedChange.VerifyBefore(context)),
            []);
    }
}
