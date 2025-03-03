namespace RoadRegistry.BackOffice.Framework;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using System;

public class Event : IRoadRegistryMessage
{
    public Event(object body)
        : this(Guid.NewGuid(),
            body ?? throw new ArgumentNullException(nameof(body)),
            default,
            default,
            default)
    {
    }

    private Event(Guid messageId, object body, string streamId, int streamVersion, ProvenanceData provenanceData)
    {
        MessageId = messageId;
        Body = body;
        StreamId = streamId;
        StreamVersion = streamVersion;
        ProvenanceData = provenanceData;
    }

    public Guid MessageId { get; }
    public object Body { get; }
    public string StreamId { get; }
    public int StreamVersion { get; }
    public ProvenanceData ProvenanceData { get; }

    public Event WithMessageId(Guid value)
    {
        return new Event(value, Body, StreamId, StreamVersion, ProvenanceData);
    }

    public Event WithStream(string streamId, int streamVersion)
    {
        return new Event(MessageId, Body, streamId, streamVersion, ProvenanceData);
    }

    public Event WithProvenanceData(ProvenanceData? value)
    {
        return value is not null
            ? new Event(MessageId, Body, StreamId, StreamVersion, value)
            : this;
    }
}
