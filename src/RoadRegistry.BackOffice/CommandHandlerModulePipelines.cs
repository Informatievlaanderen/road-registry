namespace RoadRegistry.BackOffice;

using Autofac;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Core;
using FluentValidation;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SqlStreamStore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public static class CommandHandlerModulePipelines
{
    private static readonly EventMapping EventMapping =
        new(EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly));

    private static readonly JsonSerializerSettings SerializerSettings =
        EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

    public static ICommandHandlerBuilder<IRoadRegistryContext, TCommand> UseRoadRegistryContext<TCommand>(
        this ICommandHandlerBuilder<TCommand> builder, IStreamStore store, ILifetimeScope lifetimeScope, IRoadNetworkSnapshotReader snapshotReader, ILoggerFactory loggerFactory, EventEnricher enricher)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(snapshotReader);
        ArgumentNullException.ThrowIfNull(enricher);

        return builder.Pipe<IRoadRegistryContext>(next => async (message, commandMetadata, ct) =>
            {
                await HandleMessage(store, lifetimeScope, snapshotReader, enricher, context => next(context, message, commandMetadata, ct), message, loggerFactory, ct);
            }
        );
    }

    public static IEventHandlerBuilder<IRoadRegistryContext, TCommand> UseRoadRegistryContext<TCommand>(
        this IEventHandlerBuilder<TCommand> builder,
        IStreamStore store,
        ILifetimeScope lifetimeScope,
        IRoadNetworkSnapshotReader snapshotReader,
        ILoggerFactory loggerFactory,
        EventEnricher enricher)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(snapshotReader);
        ArgumentNullException.ThrowIfNull(enricher);

        return builder.Pipe<IRoadRegistryContext>(next => async (message, ct) =>
            {
                await HandleMessage(store, lifetimeScope, snapshotReader, enricher, context => next(context, message, ct), message, loggerFactory, ct);
            }
        );
    }

    private static async Task HandleMessage<TMessage>(
        IStreamStore store,
        ILifetimeScope lifetimeScope,
        IRoadNetworkSnapshotReader snapshotReader,
        EventEnricher enricher,
        Func<IRoadRegistryContext, Task> next,
        TMessage message,
        ILoggerFactory loggerFactory,
        CancellationToken ct)
        where TMessage : IRoadRegistryMessage
    {
        using (var container = lifetimeScope.BeginLifetimeScope())
        {
            var map = container.Resolve<EventSourcedEntityMap>();
       
            var context = new RoadRegistryContext(map, store, snapshotReader, SerializerSettings, EventMapping, loggerFactory);

            await next(context);

            IRoadNetworkEventWriter roadNetworkEventWriter = new RoadNetworkEventWriter(store, enricher);
            
            foreach (var entry in map.Entries)
            {
                var events = entry.Entity.TakeEvents();
                if (events.Length != 0)
                {
                    await roadNetworkEventWriter.WriteAsync(entry.Stream, message, entry.ExpectedVersion, events, ct);
                }
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
