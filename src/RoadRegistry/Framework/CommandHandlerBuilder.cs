namespace RoadRegistry.Framework
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class CommandHandlerBuilder<TContext, TCommand>
      : ICommandHandlerBuilder<TContext, TCommand>
    {
        internal CommandHandlerBuilder(Action<Func<TContext, Message<TCommand>, CancellationToken, Task>> builder)
        {
            Builder = builder ??
              throw new ArgumentNullException(nameof(builder));
        }
        public Action<Func<TContext, Message<TCommand>, CancellationToken, Task>> Builder { get; }

        public void Handle(Func<TContext, Message<TCommand>, CancellationToken, Task> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            Builder(handler);
        }

        public ICommandHandlerBuilder<TContext, TCommand> Pipe(
        Func<
          Func<TContext, Message<TCommand>, CancellationToken, Task>,
          Func<TContext, Message<TCommand>, CancellationToken, Task>> pipe)
        {
            if (pipe == null)
                throw new ArgumentNullException(nameof(pipe));

            return new WithPipeline(Builder, next => pipe(next));
        }

        private class WithPipeline : ICommandHandlerBuilder<TContext, TCommand>
        {
            public WithPipeline(
              Action<Func<TContext, Message<TCommand>, CancellationToken, Task>> builder,
              Func<
                Func<TContext, Message<TCommand>, CancellationToken, Task>,
                Func<TContext, Message<TCommand>, CancellationToken, Task>> pipeline)
            {
                Builder = builder;
                Pipeline = pipeline;
            }

            public Action<Func<TContext, Message<TCommand>, CancellationToken, Task>> Builder { get; }

            public Func<
              Func<TContext, Message<TCommand>, CancellationToken, Task>,
              Func<TContext, Message<TCommand>, CancellationToken, Task>> Pipeline
            { get; }

            public ICommandHandlerBuilder<TContext, TCommand> Pipe(
                Func<
                    Func<TContext, Message<TCommand>, CancellationToken, Task>,
                    Func<TContext, Message<TCommand>, CancellationToken, Task>> pipe)
            {
                if (pipe == null)
                    throw new ArgumentNullException(nameof(pipe));

                return new WithPipeline(Builder, next => Pipeline(pipe(next)));
            }

            public void Handle(Func<TContext, Message<TCommand>, CancellationToken, Task> handler)
            {
                if (handler == null)
                    throw new ArgumentNullException(nameof(handler));

                Builder(Pipeline(handler));
            }
        }
    }
}
