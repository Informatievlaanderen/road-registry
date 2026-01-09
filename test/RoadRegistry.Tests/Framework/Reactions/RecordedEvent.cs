namespace RoadRegistry.Tests.Framework.Reactions;

using RoadRegistry.BackOffice.Framework;

public class RecordedEvent
{
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

    public object Event { get; }
    public Guid MessageId { get; }
    public object Metadata { get; }
    public StreamName Stream { get; }

    public RecordedEvent WithMessageId(Guid value)
    {
        return new RecordedEvent(Stream, Event, value, Metadata);
    }

    public RecordedEvent WithMetadata(object value)
    {
        return new RecordedEvent(Stream, Event, MessageId, value);
    }
}
