namespace RoadRegistry.BackOffice.Model
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using FluentValidation;
    using Framework;
    using Messages;
    using Newtonsoft.Json;
    using SqlStreamStore;
    using SqlStreamStore.Streams;

    internal static class CommandHandlerModulePipelines
    {
        private static readonly JsonSerializerSettings Settings =
            EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
        private static readonly EventMapping Mapping =
            new EventMapping(EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly));

        public static ICommandHandlerBuilder<TCommand> UseValidator<TCommand>(
            this ICommandHandlerBuilder<TCommand> builder, IValidator<TCommand> validator)
        {
            return builder.Pipe(next => async (message, ct) =>
            {
                await validator.ValidateAndThrowAsync(message.Body, cancellationToken: ct);
                await next(message, ct);
            });
        }

        public static ICommandHandlerBuilder<IRoadRegistryContext, TCommand> UseRoadRegistryContext<TCommand>(
            this ICommandHandlerBuilder<TCommand> builder, IStreamStore store)
        {
            return builder.Pipe<IRoadRegistryContext>(next => async (message, ct) =>
                {
                    var map = new EventSourcedEntityMap();
                    var context = new RoadRegistryContext(map, store, Settings, Mapping);

                    await next(context, message, ct);

                    foreach (var entry in map.Entries)
                    {
                        var events = entry.Entity.TakeEvents();
                        if (events.Length != 0)
                        {
                            var messageId =
                                message.Head.TryGetValue("MessageId", out object value)
                                    ? value.ToString()
                                    : Guid.NewGuid().ToString("N");
                            var version = entry.ExpectedVersion;
                            var messages = Array.ConvertAll(
                                events,
                                @event =>
                                    new NewStreamMessage(
                                        Deterministic.Create(Deterministic.Namespaces.Events,
                                            $"{messageId}-{version++}"),
                                        Mapping.GetEventName(@event.GetType()),
                                        JsonConvert.SerializeObject(@event, Settings)
                                    ));
                            await store.AppendToStream(entry.Stream, entry.ExpectedVersion, messages, ct);
                        }
                    }

                }
            );
        }
    }
}
