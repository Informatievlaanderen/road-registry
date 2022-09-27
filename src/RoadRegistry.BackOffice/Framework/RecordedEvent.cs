namespace RoadRegistry.BackOffice.Framework;

using System;

public class RecordedEvent
{
    public RecordedEvent(StreamName stream, object @event)
    {
        Stream = stream;
        Event = @event ?? throw new ArgumentNullException(nameof(@event));
    }

    public StreamName Stream { get; }
    public object Event { get; }
}
