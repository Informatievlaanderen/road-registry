namespace RoadRegistry.Testing
{
    using System;
    using Framework;

    public class RecordedEvent
    {
        public StreamName Stream { get; }
        public object Event { get; }

        public RecordedEvent(StreamName stream, object @event)
        {
            Stream = stream;
            Event = @event ?? throw new ArgumentNullException(nameof(@event));
        }
    }
}
