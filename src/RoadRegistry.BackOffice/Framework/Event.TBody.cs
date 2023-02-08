namespace RoadRegistry.BackOffice.Framework;

using System;
using System.Security.Claims;

public class Event<TBody> : IRoadRegistryMessage
{
    public Event(Event @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        MessageId = @event.MessageId;
        Body = (TBody)@event.Body;
        Principal = @event.Principal;
        StreamId = @event.StreamId;
        StreamVersion = @event.StreamVersion;
    }

    public Guid MessageId { get; }
    public TBody Body { get; }
    public ClaimsPrincipal Principal { get; }
    public string StreamId { get; }
    public int StreamVersion { get; }
}
