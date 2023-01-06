namespace RoadRegistry.BackOffice.Framework;

using System;
using System.Threading;
using System.Threading.Tasks;

public interface ICommandHandlerBuilder<TCommand>
{
    void Handle(Func<Command<TCommand>, CommandMetadata, CancellationToken, Task> handler);

    ICommandHandlerBuilder<TCommand> Pipe(
        Func<
            Func<Command<TCommand>, CommandMetadata, CancellationToken, Task>,
            Func<Command<TCommand>, CommandMetadata, CancellationToken, Task>> pipe);

    ICommandHandlerBuilder<TContext, TCommand> Pipe<TContext>(
        Func<
            Func<TContext, Command<TCommand>, CommandMetadata, CancellationToken, Task>,
            Func<Command<TCommand>, CommandMetadata, CancellationToken, Task>> pipe);
}

public interface ICommandHandlerBuilder<TContext, TCommand>
{
    void Handle(Func<TContext, Command<TCommand>, CommandMetadata, CancellationToken, Task> handler);

    ICommandHandlerBuilder<TContext, TCommand> Pipe(
        Func<
            Func<TContext, Command<TCommand>, CommandMetadata, CancellationToken, Task>,
            Func<TContext, Command<TCommand>, CommandMetadata, CancellationToken, Task>> pipe);
}
