namespace RoadRegistry.BackOffice.Framework;

using System;
using System.Security.Claims;

public class Event
{
    public Event(object body)
    {
        MessageId = Guid.NewGuid();
        Principal = new ClaimsPrincipal(new ClaimsIdentity(Array.Empty<Claim>()));
        Body = body ?? throw new ArgumentNullException(nameof(body));
    }

    private Event(Guid messageId, ClaimsPrincipal principal, object body)
    {
        MessageId = messageId;
        Principal = principal;
        Body = body;
    }

    public object Body { get; }

    public Guid MessageId { get; }
    public ClaimsPrincipal Principal { get; }

    public Event WithMessageId(Guid value)
    {
        return new Event(value, Principal, Body);
    }

    public Event WithPrincipal(ClaimsPrincipal value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        return new Event(MessageId, value, Body);
    }
}
