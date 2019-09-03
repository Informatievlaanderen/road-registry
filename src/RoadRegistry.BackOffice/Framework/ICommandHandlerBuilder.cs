namespace RoadRegistry.BackOffice.Framework
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface ICommandHandlerBuilder<TCommand>
    {
        ICommandHandlerBuilder<TCommand> Pipe(
          Func<
            Func<Command<TCommand>, CancellationToken, Task>,
            Func<Command<TCommand>, CancellationToken, Task>> pipe);

        ICommandHandlerBuilder<TContext, TCommand> Pipe<TContext>(
            Func<
                Func<TContext, Command<TCommand>, CancellationToken, Task>,
                Func<Command<TCommand>, CancellationToken, Task>> pipe);

        void Handle(Func<Command<TCommand>, CancellationToken, Task> handler);
    }

    public interface ICommandHandlerBuilder<TContext, TCommand>
    {
        ICommandHandlerBuilder<TContext, TCommand> Pipe(
            Func<
                Func<TContext, Command<TCommand>, CancellationToken, Task>,
                Func<TContext, Command<TCommand>, CancellationToken, Task>> pipe);

        void Handle(Func<TContext, Command<TCommand>, CancellationToken, Task> handler);
    }
}
