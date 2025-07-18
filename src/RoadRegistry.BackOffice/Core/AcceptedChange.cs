namespace RoadRegistry.BackOffice.Core;

using System.Collections.Generic;

public class AcceptedChange : IVerifiedChange
{
    private readonly List<Messages.AcceptedChange> _acceptedChanges;

    public AcceptedChange(List<Messages.AcceptedChange> acceptedChanges)
    {
        _acceptedChanges = acceptedChanges;
    }

    public IEnumerable<Messages.AcceptedChange> Translate()
    {
        return _acceptedChanges;
    }
}
