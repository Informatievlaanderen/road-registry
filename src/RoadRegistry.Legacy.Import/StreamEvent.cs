namespace RoadRegistry.Legacy.Import
{
    using System;
    using BackOffice.Framework;

    public class StreamEvent
    {
        public StreamEvent(StreamName stream, object @event)
        {
            Stream = stream;
            Event = @event ?? throw new ArgumentNullException(nameof(@event));
        }

        public StreamName Stream { get; }
        public object Event { get; }
    }
}
