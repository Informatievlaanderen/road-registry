namespace RoadRegistry.BackOffice.Framework;

using System;
using System.Collections.Generic;

public class EventRecorder
{
    private readonly List<object> _recorded = new();

    public bool HasRecordedEvents => _recorded.Count != 0;

    public void Record(object @event)
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));

        _recorded.Add(@event);
    }

    public object[] RecordedEvents => _recorded.ToArray();

    public void Reset()
    {
        _recorded.Clear();
    }
}
