namespace RoadRegistry.BackOffice.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class EventHandlerModule
    {
        private readonly List<EventHandler> _handlers;

        protected EventHandlerModule()
        {
            _handlers = new List<EventHandler>();
        }

        public EventHandler[] Handlers => _handlers.ToArray();

        protected void Handle<TEvent>(Func<Event<TEvent>, CancellationToken, Task> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));
            _handlers.Add(
                new EventHandler(
                    typeof(TEvent),
                    (message, ct) => handler(new Event<TEvent>(message), ct)
                ));
        }

        protected IEventHandlerBuilder<TEvent> For<TEvent>()
        {
            return new EventHandlerBuilder<TEvent>(handler =>
            {
                _handlers.Add(
                    new EventHandler(
                        typeof(TEvent),
                        (message, ct) => handler(new Event<TEvent>(message), ct)
                    ));
            });
        }
    }
}
