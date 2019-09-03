namespace RoadRegistry.BackOffice
{
    using System;
    using Framework;
    using SqlStreamStore;

    internal static class EventHandlerModulePipelines
    {
        public static IEventHandlerBuilder<IRoadNetworkCommandQueue, TEvent> UseRoadNetworkCommandQueue<TEvent>(
            this IEventHandlerBuilder<TEvent> builder, IStreamStore store)
        {
            if (store == null) throw new ArgumentNullException(nameof(store));
            return builder.Pipe<IRoadNetworkCommandQueue>(next => (message, ct) => next(new RoadNetworkCommandQueue(store), message, ct));
        }
    }
}
