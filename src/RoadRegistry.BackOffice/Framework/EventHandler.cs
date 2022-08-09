namespace RoadRegistry.BackOffice.Framework;

using System;
using System.Threading;
using System.Threading.Tasks;

public class EventHandler
{
    public EventHandler(
        Type @event,
        Func<Event, CancellationToken, Task> handler)
    {
        Event = @event ?? throw new ArgumentNullException(nameof(@event));
        Handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }

    public Type Event { get; }
    public Func<Event, CancellationToken, Task> Handler { get; }
}
