namespace RoadRegistry.BackOffice.Framework
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class EventHandlerBuilder<TEvent>
      : IEventHandlerBuilder<TEvent>
    {
        internal EventHandlerBuilder(Action<Func<Event<TEvent>, CancellationToken, Task>> builder)
        {
            Builder = builder ??
              throw new ArgumentNullException(nameof(builder));
        }
        public Action<Func<Event<TEvent>, CancellationToken, Task>> Builder { get; }

        public void Handle(Func<Event<TEvent>, CancellationToken, Task> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            Builder(handler);
        }

        public IEventHandlerBuilder<TEvent> Pipe(
        Func<
          Func<Event<TEvent>, CancellationToken, Task>,
          Func<Event<TEvent>, CancellationToken, Task>> pipe)
        {
            if (pipe == null)
            {
                throw new ArgumentNullException(nameof(pipe));
            }

            return new WithPipeline(Builder, pipe);
        }

        public IEventHandlerBuilder<TContext, TEvent> Pipe<TContext>(
            Func<
                Func<TContext, Event<TEvent>, CancellationToken, Task>,
                Func<Event<TEvent>, CancellationToken, Task>> pipe)
        {
            if (pipe == null)
            {
                throw new ArgumentNullException(nameof(pipe));
            }

            return new WithContextPipeline<TContext>(Builder, pipe);
        }

        private sealed class WithPipeline : IEventHandlerBuilder<TEvent>
        {
            public WithPipeline(
              Action<Func<Event<TEvent>, CancellationToken, Task>> builder,
              Func<
                Func<Event<TEvent>, CancellationToken, Task>,
                Func<Event<TEvent>, CancellationToken, Task>> pipeline)
            {
                Builder = builder;
                Pipeline = pipeline;
            }

            private Action<Func<Event<TEvent>, CancellationToken, Task>> Builder { get; }

            private Func<
              Func<Event<TEvent>, CancellationToken, Task>,
              Func<Event<TEvent>, CancellationToken, Task>> Pipeline
            { get; }

            public IEventHandlerBuilder<TEvent> Pipe(
                Func<
                    Func<Event<TEvent>, CancellationToken, Task>,
                    Func<Event<TEvent>, CancellationToken, Task>> pipe)
            {
                if (pipe == null)
                {
                    throw new ArgumentNullException(nameof(pipe));
                }

                return new WithPipeline(Builder, next => Pipeline(pipe(next)));
            }

            public IEventHandlerBuilder<TContext, TEvent> Pipe<TContext>(
                Func<
                    Func<TContext, Event<TEvent>, CancellationToken, Task>,
                    Func<Event<TEvent>, CancellationToken, Task>> pipe)
            {
                if (pipe == null)
                {
                    throw new ArgumentNullException(nameof(pipe));
                }

                return new WithContextPipeline<TContext>(Builder, next => Pipeline(pipe(next)));
            }


            public void Handle(Func<Event<TEvent>, CancellationToken, Task> handler)
            {
                if (handler == null)
                {
                    throw new ArgumentNullException(nameof(handler));
                }

                Builder(Pipeline(handler));
            }
        }

        private sealed class WithContextPipeline<TContext> : IEventHandlerBuilder<TContext, TEvent>
        {
            public WithContextPipeline(
                Action<Func<Event<TEvent>, CancellationToken, Task>> builder,
                Func<
                    Func<TContext, Event<TEvent>, CancellationToken, Task>,
                    Func<Event<TEvent>, CancellationToken, Task>> pipeline)
            {
                Builder = builder;
                Pipeline = pipeline;
            }

            private Action<Func<Event<TEvent>, CancellationToken, Task>> Builder { get; }

            private Func<
                Func<TContext, Event<TEvent>, CancellationToken, Task>,
                Func<Event<TEvent>, CancellationToken, Task>> Pipeline
            { get; }

            public IEventHandlerBuilder<TContext, TEvent> Pipe(
                Func<
                    Func<TContext, Event<TEvent>, CancellationToken, Task>,
                    Func<TContext, Event<TEvent>, CancellationToken, Task>> pipe)
            {
                if (pipe == null)
                {
                    throw new ArgumentNullException(nameof(pipe));
                }

                return new WithContextPipeline<TContext>(Builder, next => Pipeline(pipe(next)));
            }

            public void Handle(Func<TContext, Event<TEvent>, CancellationToken, Task> handler)
            {
                if (handler == null)
                {
                    throw new ArgumentNullException(nameof(handler));
                }

                Builder(Pipeline(handler));
            }
        }
    }
}
