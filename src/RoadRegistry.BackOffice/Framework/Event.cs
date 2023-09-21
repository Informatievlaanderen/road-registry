namespace RoadRegistry.BackOffice.Framework;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using System;
using System.Security.Claims;

public class Event : IRoadRegistryMessage
{
    public Event(object body)
        : this(Guid.NewGuid(),
            new ClaimsPrincipal(new ClaimsIdentity(Array.Empty<System.Security.Claims.Claim>())),
            body ?? throw new ArgumentNullException(nameof(body)),
            default,
            default,
            default)
    {
    }

    private Event(Guid messageId, ClaimsPrincipal principal, object body, string streamId, int streamVersion, ProvenanceData provenanceData)
    {
        MessageId = messageId;
        Principal = principal;
        Body = body;
        StreamId = streamId;
        StreamVersion = streamVersion;
    }

    public Guid MessageId { get; }
    public object Body { get; }
    public ClaimsPrincipal Principal { get; }
    public string StreamId { get; }
    public int StreamVersion { get; }
    public ProvenanceData ProvenanceData { get; }

    public Event WithMessageId(Guid value)
    {
        return new Event(value, Principal, Body, StreamId, StreamVersion, ProvenanceData);
    }

    public Event WithStream(string streamId, int streamVersion)
    {
        return new Event(MessageId, Principal, Body, streamId, streamVersion, ProvenanceData);
    }
    
    public Event WithPrincipal(ClaimsPrincipal value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new Event(MessageId, value, Body, StreamId, StreamVersion, ProvenanceData);
    }

    public Event WithProvenanceData(ProvenanceData value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new Event(MessageId, Principal, Body, StreamId, StreamVersion, ProvenanceData);
    }
}
