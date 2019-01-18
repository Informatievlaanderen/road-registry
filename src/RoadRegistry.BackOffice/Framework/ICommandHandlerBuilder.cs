namespace RoadRegistry.BackOffice.Framework
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface ICommandHandlerBuilder<TCommand>
    {
        ICommandHandlerBuilder<TCommand> Pipe(
          Func<
            Func<Message<TCommand>, CancellationToken, Task>,
            Func<Message<TCommand>, CancellationToken, Task>> pipe);

        ICommandHandlerBuilder<TContext, TCommand> Pipe<TContext>(
            Func<
                Func<TContext, Message<TCommand>, CancellationToken, Task>,
                Func<Message<TCommand>, CancellationToken, Task>> pipe);

        void Handle(Func<Message<TCommand>, CancellationToken, Task> handler);
    }

    public interface ICommandHandlerBuilder<TContext, TCommand>
    {
        ICommandHandlerBuilder<TContext, TCommand> Pipe(
            Func<
                Func<TContext, Message<TCommand>, CancellationToken, Task>,
                Func<TContext, Message<TCommand>, CancellationToken, Task>> pipe);

        void Handle(Func<TContext, Message<TCommand>, CancellationToken, Task> handler);
    }
}
