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
    }

    public Guid MessageId { get; }
    public TBody Body { get; }
    public ClaimsPrincipal Principal { get; }
}
