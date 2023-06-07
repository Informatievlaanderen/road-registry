namespace RoadRegistry.BackOffice;

using System;
using Framework;
using SqlStreamStore;

public static class EventHandlerModulePipelines
{
    public static IEventHandlerBuilder<IRoadNetworkCommandQueue, TEvent> UseRoadNetworkCommandQueue<TEvent>(
        this IEventHandlerBuilder<TEvent> builder, IStreamStore store, ApplicationMetadata applicationMetadata)
    {
        if (store == null) throw new ArgumentNullException(nameof(store));
        return builder.Pipe<IRoadNetworkCommandQueue>(next => (message, ct) => next(new RoadNetworkCommandQueue(store, applicationMetadata), message, ct));
    }

    public static IEventHandlerBuilder<IRoadNetworkExtractCommandQueue, TEvent> UseRoadNetworkExtractCommandQueue<TEvent>(
        this IEventHandlerBuilder<TEvent> builder, IStreamStore store)
    {
        if (store == null) throw new ArgumentNullException(nameof(store));
        return builder.Pipe<IRoadNetworkExtractCommandQueue>(next => (message, ct) => next(new RoadNetworkExtractCommandQueue(store), message, ct));
    }
}
