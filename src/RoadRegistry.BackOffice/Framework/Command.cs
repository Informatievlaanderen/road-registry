namespace RoadRegistry.BackOffice.Framework;

using System;
using System.Security.Claims;

public class Command
{
    public Command(object body)
    {
        MessageId = Guid.NewGuid();
        Principal = new ClaimsPrincipal(new ClaimsIdentity(Array.Empty<Claim>()));
        Body = body ?? throw new ArgumentNullException(nameof(body));
    }

    private Command(Guid messageId, ClaimsPrincipal principal, object body)
    {
        MessageId = messageId;
        Principal = principal;
        Body = body;
    }

    public Guid MessageId { get; }
    public ClaimsPrincipal Principal { get; }
    public object Body { get; }

    public Command WithMessageId(Guid value)
    {
        return new Command(value, Principal, Body);
    }

    public Command WithPrincipal(ClaimsPrincipal value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        return new Command(MessageId, value, Body);
    }
}
