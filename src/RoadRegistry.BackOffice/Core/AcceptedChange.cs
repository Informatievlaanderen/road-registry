namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Linq;

public class AcceptedChange : IVerifiedChange
{
    private readonly Problems _problems;
    private readonly IRequestedChange _requestedChange;

    public AcceptedChange(IRequestedChange change, Problems problems)
    {
        _requestedChange = change ?? throw new ArgumentNullException(nameof(change));
        _problems = problems ?? throw new ArgumentNullException(nameof(problems));
    }

    // public IEnumerable<Messages.AcceptedChange> Translate()
    // {
    //     var warnings = _problems.Select(warning => warning.Translate()).ToArray();
    //     return _requestedChange.TranslateTo(warnings);
    // }
}
