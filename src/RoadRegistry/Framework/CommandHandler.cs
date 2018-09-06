namespace RoadRegistry.Framework
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class CommandHandler<TContext>
    {
        public CommandHandler(
            Type command,
            Func<TContext, Message, CancellationToken, Task> handler)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
            Handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        public Type Command { get; }
        public Func<TContext, Message, CancellationToken, Task> Handler { get; }
    }
}
