namespace RoadRegistry.Framework
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface ICommandHandlerBuilder<TContext, TCommand>
    {
        ICommandHandlerBuilder<TContext, TCommand> Pipe(
          Func<
            Func<TContext, Message<TCommand>, CancellationToken, Task>,
            Func<TContext, Message<TCommand>, CancellationToken, Task>> pipe);

        void Handle(Func<TContext, Message<TCommand>, CancellationToken, Task> handler);
    }
}
