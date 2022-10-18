namespace RoadRegistry.BackOffice.Core;

using System;
using System.Linq;

public class RejectedChange : IVerifiedChange
{
    public RejectedChange(IRequestedChange change, Problems problems)
    {
        _requestedChange = change ?? throw new ArgumentNullException(nameof(change));
        _problems = problems ?? throw new ArgumentNullException(nameof(problems));
    }

    private readonly Problems _problems;
    private readonly IRequestedChange _requestedChange;

    public Messages.RejectedChange Translate()
    {
        var message = new Messages.RejectedChange
        {
            Problems = _problems.Select(problem => problem.Translate()).ToArray()
        };
        _requestedChange.TranslateTo(message);
        return message;
    }
}
