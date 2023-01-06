namespace RoadRegistry.BackOffice.Framework;

using System;
using System.Security.Claims;

public class Event : IRoadRegistryMessage
{
    public Event(object body)
        : this(Guid.NewGuid(),
            new ClaimsPrincipal(new ClaimsIdentity(Array.Empty<System.Security.Claims.Claim>())),
            body ?? throw new ArgumentNullException(nameof(body)))
    {
    }

    private Event(Guid messageId, ClaimsPrincipal principal, object body)
    {
        MessageId = messageId;
        Principal = principal;
        Body = body;
    }

    public Guid MessageId { get; }
    public object Body { get; }
    public ClaimsPrincipal Principal { get; }

    public Event WithMessageId(Guid value)
    {
        return new Event(value, Principal, Body);
    }
    
    public Event WithPrincipal(ClaimsPrincipal value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new Event(MessageId, value, Body);
    }
}
