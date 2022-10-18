namespace RoadRegistry.BackOffice.Framework;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public abstract class EventHandlerModule
{
    protected EventHandlerModule()
    {
        _handlers = new List<EventHandler>();
    }

    private readonly List<EventHandler> _handlers;

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

    public EventHandler[] Handlers => _handlers.ToArray();
}
