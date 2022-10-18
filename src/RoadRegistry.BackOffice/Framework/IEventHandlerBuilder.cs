namespace RoadRegistry.BackOffice.Framework;

using System;
using System.Threading;
using System.Threading.Tasks;

public interface IEventHandlerBuilder<TEvent>
{
    void Handle(Func<Event<TEvent>, CancellationToken, Task> handler);

    IEventHandlerBuilder<TEvent> Pipe(
        Func<
            Func<Event<TEvent>, CancellationToken, Task>,
            Func<Event<TEvent>, CancellationToken, Task>> pipe);

    IEventHandlerBuilder<TContext, TEvent> Pipe<TContext>(
        Func<
            Func<TContext, Event<TEvent>, CancellationToken, Task>,
            Func<Event<TEvent>, CancellationToken, Task>> pipe);
}

public interface IEventHandlerBuilder<TContext, TEvent>
{
    void Handle(Func<TContext, Event<TEvent>, CancellationToken, Task> handler);

    IEventHandlerBuilder<TContext, TEvent> Pipe(
        Func<
            Func<TContext, Event<TEvent>, CancellationToken, Task>,
            Func<TContext, Event<TEvent>, CancellationToken, Task>> pipe);
}
