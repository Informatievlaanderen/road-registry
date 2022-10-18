namespace RoadRegistry.BackOffice.Framework;

using System;
using System.Threading;
using System.Threading.Tasks;

public class CommandHandler
{
    public CommandHandler(
        Type command,
        Func<Command, CancellationToken, Task> handler)
    {
        Command = command ?? throw new ArgumentNullException(nameof(command));
        Handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }

    public Type Command { get; }
    public Func<Command, CancellationToken, Task> Handler { get; }
}