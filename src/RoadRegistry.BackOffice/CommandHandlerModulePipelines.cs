namespace RoadRegistry.BackOffice;

using System;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.Generators.Guid;
using Core;
using FluentValidation;
using Framework;
using Messages;
using Newtonsoft.Json;
using SqlStreamStore;
using SqlStreamStore.Streams;

internal static class CommandHandlerModulePipelines
{
    private static readonly JsonSerializerSettings SerializerSettings =
        EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

    private static readonly EventMapping EventMapping =
        new(EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly));

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
        this ICommandHandlerBuilder<TCommand> builder, IStreamStore store, IRoadNetworkSnapshotReader snapshotReader, EventEnricher enricher)
    {
        if (store == null) throw new ArgumentNullException(nameof(store));
        if (snapshotReader == null) throw new ArgumentNullException(nameof(snapshotReader));
        if (enricher == null) throw new ArgumentNullException(nameof(enricher));
        return builder.Pipe<IRoadRegistryContext>(next => async (message, ct) =>
            {
                var map = new EventSourcedEntityMap();
                var context = new RoadRegistryContext(map, store, snapshotReader, SerializerSettings, EventMapping);

                await next(context, message, ct);

                foreach (var entry in map.Entries)
                {
                    var events = entry.Entity.TakeEvents();
                    if (events.Length != 0)
                    {
                        var messageId = message.MessageId.ToString("N");
                        var version = entry.ExpectedVersion;
                        Array.ForEach(events, @event => enricher(@event));
                        var messages = Array.ConvertAll(
                            events,
                            @event =>
                                new NewStreamMessage(
                                    Deterministic.Create(Deterministic.Namespaces.Events,
                                        $"{messageId}-{version++}"),
                                    EventMapping.GetEventName(@event.GetType()),
                                    JsonConvert.SerializeObject(@event, SerializerSettings)
                                ));
                        await store.AppendToStream(entry.Stream, entry.ExpectedVersion, messages, ct);
                    }
                }
            }
        );
    }

    public static IEventHandlerBuilder<IRoadRegistryContext, TCommand> UseRoadRegistryContext<TCommand>(
        this IEventHandlerBuilder<TCommand> builder, IStreamStore store, IRoadNetworkSnapshotReader snapshotReader, EventEnricher enricher)
    {
        if (store == null) throw new ArgumentNullException(nameof(store));
        if (enricher == null) throw new ArgumentNullException(nameof(enricher));
        return builder.Pipe<IRoadRegistryContext>(next => async (message, ct) =>
            {
                var map = new EventSourcedEntityMap();
                var context = new RoadRegistryContext(map, store, snapshotReader, SerializerSettings, EventMapping);

                await next(context, message, ct);

                foreach (var entry in map.Entries)
                {
                    var events = entry.Entity.TakeEvents();
                    if (events.Length != 0)
                    {
                        var messageId = message.MessageId.ToString("N");
                        var version = entry.ExpectedVersion;
                        Array.ForEach(events, @event => enricher(@event));
                        var messages = Array.ConvertAll(
                            events,
                            @event =>
                                new NewStreamMessage(
                                    Deterministic.Create(Deterministic.Namespaces.Events,
                                        $"{messageId}-{version++}"),
                                    EventMapping.GetEventName(@event.GetType()),
                                    JsonConvert.SerializeObject(@event, SerializerSettings)
                                ));
                        await store.AppendToStream(entry.Stream, entry.ExpectedVersion, messages, ct);
                    }
                }
            }
        );
    }
}
