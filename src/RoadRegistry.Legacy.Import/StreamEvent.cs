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

        public object Event { get; }

        public StreamName Stream { get; }
    }
}
