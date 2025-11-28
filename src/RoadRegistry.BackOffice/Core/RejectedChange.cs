namespace RoadRegistry.BackOffice.Core;

using System;
using System.Linq;
using CommandHandling.Actions.ChangeRoadNetwork;
using ValueObjects.Problems;

public class RejectedChange : IVerifiedChange
{
    private readonly Problems _problems;
    private readonly IRequestedChange _requestedChange;

    public RejectedChange(IRequestedChange change, Problems problems)
    {
        _requestedChange = change ?? throw new ArgumentNullException(nameof(change));
        _problems = problems ?? throw new ArgumentNullException(nameof(problems));
    }

    public Messages.RejectedChange Translate()
    {
        var message = new Messages.RejectedChange
        {
            Problems = _problems.Select(problem => problem.Translate()).ToArray()
        };
        _requestedChange.TranslateToRejectedChange(message);
        return message;
    }
}
