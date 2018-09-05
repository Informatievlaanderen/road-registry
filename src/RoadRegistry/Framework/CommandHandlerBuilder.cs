namespace RoadRegistry.Framework
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class CommandHandlerBuilder<TCommand>
      : ICommandHandlerBuilder<TCommand>
    {
        internal CommandHandlerBuilder(Action<Func<Message<TCommand>, CancellationToken, Task>> builder)
        {
            Builder = builder ??
              throw new ArgumentNullException(nameof(builder));
        }
        public Action<Func<Message<TCommand>, CancellationToken, Task>> Builder { get; }

        public void Handle(Func<Message<TCommand>, CancellationToken, Task> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            Builder(handler);
        }

        public ICommandHandlerBuilder<TCommand> Pipe(
        Func<
          Func<Message<TCommand>, CancellationToken, Task>,
          Func<Message<TCommand>, CancellationToken, Task>> pipe)
        {
            if (pipe == null)
                throw new ArgumentNullException(nameof(pipe));

            return new WithPipeline(Builder, pipe);
      }

        private class WithPipeline : ICommandHandlerBuilder<TCommand>
        {
            public WithPipeline(
              Action<Func<Message<TCommand>, CancellationToken, Task>> builder,
              Func<
                Func<Message<TCommand>, CancellationToken, Task>,
                Func<Message<TCommand>, CancellationToken, Task>> pipeline)
            {
                Builder = builder;
                Pipeline = pipeline;
            }

            public Action<Func<Message<TCommand>, CancellationToken, Task>> Builder { get; }
            public Func<
              Func<Message<TCommand>, CancellationToken, Task>,
              Func<Message<TCommand>, CancellationToken, Task>> Pipeline
            { get; }

            public ICommandHandlerBuilder<TCommand> Pipe(
        Func<
          Func<Message<TCommand>, CancellationToken, Task>,
          Func<Message<TCommand>, CancellationToken, Task>> pipe)
            {
                if (pipe == null)
                    throw new ArgumentNullException(nameof(pipe));

                return new WithPipeline(Builder, next => Pipeline(pipe(next)));
            }
            public void Handle(Func<Message<TCommand>, CancellationToken, Task> handler)
            {
                if (handler == null)
                    throw new ArgumentNullException(nameof(handler));

                Builder(Pipeline(handler));
            }
        }
    }
}
