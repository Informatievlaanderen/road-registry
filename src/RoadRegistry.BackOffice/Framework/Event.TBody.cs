namespace RoadRegistry.BackOffice.Framework;

using System;
using System.Security.Claims;

public class Event<TBody>
{
    public Event(Event @event)
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));
        MessageId = @event.MessageId;
        Principal = @event.Principal;
        Body = (TBody)@event.Body;
    }

    public TBody Body { get; }

    public Guid MessageId { get; }
    public ClaimsPrincipal Principal { get; }
}
