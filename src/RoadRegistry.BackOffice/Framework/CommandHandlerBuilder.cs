namespace RoadRegistry.BackOffice.Framework;

using System;
using System.Threading;
using System.Threading.Tasks;

public class CommandHandlerBuilder<TCommand>
    : ICommandHandlerBuilder<TCommand>
{
    internal CommandHandlerBuilder(Action<Func<Command<TCommand>, ApplicationMetadata, CancellationToken, Task>> builder)
    {
        Builder = builder ??
                  throw new ArgumentNullException(nameof(builder));
    }

    public Action<Func<Command<TCommand>, ApplicationMetadata, CancellationToken, Task>> Builder { get; }

    public void Handle(Func<Command<TCommand>, ApplicationMetadata, CancellationToken, Task> handler)
    {
        if (handler == null) throw new ArgumentNullException(nameof(handler));

        Builder(handler);
    }

    public ICommandHandlerBuilder<TCommand> Pipe(
        Func<
            Func<Command<TCommand>, ApplicationMetadata, CancellationToken, Task>,
            Func<Command<TCommand>, ApplicationMetadata, CancellationToken, Task>> pipe)
    {
        if (pipe == null) throw new ArgumentNullException(nameof(pipe));

        return new WithPipeline(Builder, pipe);
    }

    public ICommandHandlerBuilder<TContext, TCommand> Pipe<TContext>(
        Func<
            Func<TContext, Command<TCommand>, ApplicationMetadata, CancellationToken, Task>,
            Func<Command<TCommand>, ApplicationMetadata, CancellationToken, Task>> pipe)
    {
        if (pipe == null) throw new ArgumentNullException(nameof(pipe));

        return new WithContextPipeline<TContext>(Builder, pipe);
    }

    private sealed class WithContextPipeline<TContext> : ICommandHandlerBuilder<TContext, TCommand>
    {
        public WithContextPipeline(
            Action<Func<Command<TCommand>, ApplicationMetadata, CancellationToken, Task>> builder,
            Func<
                Func<TContext, Command<TCommand>, ApplicationMetadata, CancellationToken, Task>,
                Func<Command<TCommand>, ApplicationMetadata, CancellationToken, Task>> pipeline)
        {
            Builder = builder;
            Pipeline = pipeline;
        }

        private Action<Func<Command<TCommand>, ApplicationMetadata, CancellationToken, Task>> Builder { get; }

        private Func<
            Func<TContext, Command<TCommand>, ApplicationMetadata, CancellationToken, Task>,
            Func<Command<TCommand>, ApplicationMetadata, CancellationToken, Task>> Pipeline { get; }

        public void Handle(Func<TContext, Command<TCommand>, ApplicationMetadata, CancellationToken, Task> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            Builder(Pipeline(handler));
        }

        public ICommandHandlerBuilder<TContext, TCommand> Pipe(
            Func<
                Func<TContext, Command<TCommand>, ApplicationMetadata, CancellationToken, Task>,
                Func<TContext, Command<TCommand>, ApplicationMetadata, CancellationToken, Task>> pipe)
        {
            if (pipe == null) throw new ArgumentNullException(nameof(pipe));

            return new WithContextPipeline<TContext>(Builder, next => Pipeline(pipe(next)));
        }
    }

    private sealed class WithPipeline : ICommandHandlerBuilder<TCommand>
    {
        public WithPipeline(
            Action<Func<Command<TCommand>, ApplicationMetadata, CancellationToken, Task>> builder,
            Func<
                Func<Command<TCommand>, ApplicationMetadata, CancellationToken, Task>,
                Func<Command<TCommand>, ApplicationMetadata, CancellationToken, Task>> pipeline)
        {
            Builder = builder;
            Pipeline = pipeline;
        }

        private Action<Func<Command<TCommand>, ApplicationMetadata, CancellationToken, Task>> Builder { get; }

        private Func<
            Func<Command<TCommand>, ApplicationMetadata, CancellationToken, Task>,
            Func<Command<TCommand>, ApplicationMetadata, CancellationToken, Task>> Pipeline { get; }

        public void Handle(Func<Command<TCommand>, ApplicationMetadata, CancellationToken, Task> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            Builder(Pipeline(handler));
        }

        public ICommandHandlerBuilder<TCommand> Pipe(
            Func<
                Func<Command<TCommand>, ApplicationMetadata, CancellationToken, Task>,
                Func<Command<TCommand>, ApplicationMetadata, CancellationToken, Task>> pipe)
        {
            if (pipe == null) throw new ArgumentNullException(nameof(pipe));

            return new WithPipeline(Builder, next => Pipeline(pipe(next)));
        }

        public ICommandHandlerBuilder<TContext, TCommand> Pipe<TContext>(
            Func<
                Func<TContext, Command<TCommand>, ApplicationMetadata, CancellationToken, Task>,
                Func<Command<TCommand>, ApplicationMetadata, CancellationToken, Task>> pipe)
        {
            if (pipe == null) throw new ArgumentNullException(nameof(pipe));

            return new WithContextPipeline<TContext>(Builder, next => Pipeline(pipe(next)));
        }
    }
}
