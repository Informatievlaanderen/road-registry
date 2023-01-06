namespace RoadRegistry.BackOffice.Framework;

using System;
using System.Security.Claims;

public class Command<TBody>: IRoadRegistryMessage
{
    public Command(Command command)
    {
        ArgumentNullException.ThrowIfNull(command);
        
        MessageId = command.MessageId;
        Body = (TBody)command.Body;
        Principal = command.Principal;
    }

    public Guid MessageId { get; }
    public TBody Body { get; }
    public ClaimsPrincipal Principal { get; }
}

public class CommandMetadata
{
    public CommandMetadata(RoadRegistryApplication processor)
    {
        Processor = processor;
    }

    public RoadRegistryApplication Processor { get; }
}
