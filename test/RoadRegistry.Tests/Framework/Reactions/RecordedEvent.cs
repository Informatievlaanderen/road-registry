﻿namespace RoadRegistry.Framework.Reactions
{
    using System;
    using BackOffice.Framework;

    public class RecordedEvent
    {
        public StreamName Stream { get; }
        public object Event { get; }

        public Guid MessageId { get; }

        public object Metadata { get; }

        public RecordedEvent(StreamName stream, object @event)
        {
            Stream = stream;
            Event = @event ?? throw new ArgumentNullException(nameof(@event));
            MessageId = Guid.NewGuid();
            Metadata = null;
        }

        private RecordedEvent(StreamName stream, object @event, Guid messageId, object metadata)
        {
            Stream = stream;
            Event = @event ?? throw new ArgumentNullException(nameof(@event));
            MessageId = messageId;
            Metadata = metadata;
        }

        public RecordedEvent WithMessageId(Guid value)
        {
            return new RecordedEvent(Stream, Event, value, Metadata);
        }

        public RecordedEvent WithMetadata(object value)
        {
            return new RecordedEvent(Stream, Event, MessageId, value);
        }
    }
}
