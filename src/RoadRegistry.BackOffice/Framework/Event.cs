namespace RoadRegistry.BackOffice.Framework;

using System;
using System.Security.Claims;
using SqlStreamStore.Streams;

public class Event : IRoadRegistryMessage
{
    public Event(object body)
        : this(Guid.NewGuid(),
            new ClaimsPrincipal(new ClaimsIdentity(Array.Empty<System.Security.Claims.Claim>())),
            body ?? throw new ArgumentNullException(nameof(body)),
    default, default)
    {
    }

    private Event(Guid messageId, ClaimsPrincipal principal, object body, string streamId, int streamVersion)
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

    public Event WithMessageId(Guid value)
    {
        return new Event(value, Principal, Body, StreamId, StreamVersion);
    }

    public Event WithStream(string streamId, int streamVersion)
    {
        return new Event(MessageId, Principal, Body, streamId, streamVersion);
    }
    
    public Event WithPrincipal(ClaimsPrincipal value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new Event(MessageId, value, Body, StreamId, StreamVersion);
    }
}
