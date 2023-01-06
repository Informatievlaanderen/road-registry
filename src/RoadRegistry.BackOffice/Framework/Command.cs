namespace RoadRegistry.BackOffice.Framework;

using System;
using System.Security.Claims;

public class Command: IRoadRegistryMessage
{
    public Command(object body)
        : this(Guid.NewGuid(),
            new ClaimsPrincipal(new ClaimsIdentity(Array.Empty<System.Security.Claims.Claim>())),
            body ?? throw new ArgumentNullException(nameof(body))
        )
    {
    }

    private Command(Guid messageId, ClaimsPrincipal principal, object body)
    {
        MessageId = messageId;
        Principal = principal;
        Body = body;
    }

    public object Body { get; }
    public Guid MessageId { get; }
    public ClaimsPrincipal Principal { get; }

    public Command WithMessageId(Guid value)
    {
        return new Command(value, Principal, Body);
    }
    
    public Command WithPrincipal(ClaimsPrincipal value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new Command(MessageId, value, Body);
    }
}
