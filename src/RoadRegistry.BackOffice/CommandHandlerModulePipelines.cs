namespace RoadRegistry.BackOffice;

using System;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Core;
using FluentValidation;
using Framework;
using Messages;
using Newtonsoft.Json;
using SqlStreamStore;

internal static class CommandHandlerModulePipelines
{
    private static readonly EventMapping EventMapping =
        new(EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly));

    private static readonly JsonSerializerSettings SerializerSettings =
        EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

    public static ICommandHandlerBuilder<IRoadRegistryContext, TCommand> UseRoadRegistryContext<TCommand>(
        this ICommandHandlerBuilder<TCommand> builder, IStreamStore store, Func<EventSourcedEntityMap> entityMapFactory, IRoadNetworkSnapshotReader snapshotReader, EventEnricher enricher)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(snapshotReader);
        ArgumentNullException.ThrowIfNull(enricher);

        return builder.Pipe<IRoadRegistryContext>(next => async (message, commandMetadata, ct) =>
            {
                await HandleMessage(store, entityMapFactory, snapshotReader, enricher, context => next(context, message, commandMetadata, ct), message, ct);
            }
        );
    }
    
    public static IEventHandlerBuilder<IRoadRegistryContext, TCommand> UseRoadRegistryContext<TCommand>(
        this IEventHandlerBuilder<TCommand> builder, IStreamStore store, IRoadNetworkSnapshotReader snapshotReader, EventEnricher enricher)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(snapshotReader);
        ArgumentNullException.ThrowIfNull(enricher);

        return builder.Pipe<IRoadRegistryContext>(next => async (message, ct) =>
            {
                await HandleMessage(store, () => new EventSourcedEntityMap(), snapshotReader, enricher, context => next(context, message, ct), message, ct);
            }
        );
    }

    private static async Task HandleMessage<TMessage>(
        IStreamStore store,
        Func<EventSourcedEntityMap> entityMapFactory,
        IRoadNetworkSnapshotReader snapshotReader,
        EventEnricher enricher,
        Func<IRoadRegistryContext, Task> next,
        TMessage message,
        CancellationToken ct)
        where TMessage : IRoadRegistryMessage
    {
        var map = entityMapFactory();
        var context = new RoadRegistryContext(map, store, snapshotReader, SerializerSettings, EventMapping);

        await next(context);

        var roadNetworkEventWriter = new RoadNetworkEventWriter(store, enricher);

        foreach (var entry in map.Entries)
        {
            var events = entry.Entity.TakeEvents();
            if (events.Length != 0)
            {
                await roadNetworkEventWriter.Write(entry.Stream, message, entry.ExpectedVersion, events, ct);
            }
        }
    }

    public static ICommandHandlerBuilder<TCommand> UseValidator<TCommand>(
        this ICommandHandlerBuilder<TCommand> builder, IValidator<TCommand> validator)
    {
        return builder.Pipe(next => async (message, commandMetadata, ct) =>
        {
            await validator.ValidateAndThrowAsync(message.Body, cancellationToken: ct);
            await next(message, commandMetadata, ct);
        });
    }
}
