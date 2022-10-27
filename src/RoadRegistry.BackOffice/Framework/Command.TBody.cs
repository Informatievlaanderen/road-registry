namespace RoadRegistry.BackOffice.Framework;

using System;
using System.Security.Claims;

public class Command<TBody>
{
    public Command(Command command)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));
        MessageId = command.MessageId;
        Principal = command.Principal;
        Body = (TBody)command.Body;
    }

    public TBody Body { get; }
    public Guid MessageId { get; }
    public ClaimsPrincipal Principal { get; }
}