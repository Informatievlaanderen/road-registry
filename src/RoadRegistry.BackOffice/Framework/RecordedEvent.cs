namespace RoadRegistry.BackOffice.Framework;

using System;

public class RecordedEvent
{
    public RecordedEvent(StreamName stream, object @event)
    {
        Stream = stream;
        Event = @event ?? throw new ArgumentNullException(nameof(@event));
    }

    public object Event { get; }
    public StreamName Stream { get; }
}
