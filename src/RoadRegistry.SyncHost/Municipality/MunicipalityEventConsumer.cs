namespace RoadRegistry.SyncHost;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using BackOffice;
using BackOffice.Core;
using BackOffice.Framework;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Hosts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SqlStreamStore;
using Sync.MunicipalityRegistry;
using Resolve = Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector.Resolve;

public class MunicipalityEventConsumer : RoadRegistryBackgroundService
{
    private readonly IDbContextFactory<MunicipalityEventConsumerContext> _municipalityEventConsumerDbContextFactory;
    private readonly ILifetimeScope _container;
    private readonly IStreamStore _store;
    private readonly IMunicipalityEventWriter _eventWriter;
    private readonly IMunicipalityEventTopicConsumer _consumer;

    private static readonly EventMapping EventMapping =
        new(EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly));

    private static readonly JsonSerializerSettings SerializerSettings =
        EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

    public MunicipalityEventConsumer(
        ILifetimeScope container,
        IDbContextFactory<MunicipalityEventConsumerContext> municipalityEventConsumerDbContextFactory,
        IStreamStore store,
        IMunicipalityEventWriter eventWriter,
        IMunicipalityEventTopicConsumer consumer,
        ILogger<MunicipalityEventConsumer> logger
    ) : base(logger)
    {
        _container = container.ThrowIfNull();
        _municipalityEventConsumerDbContextFactory = municipalityEventConsumerDbContextFactory.ThrowIfNull();
        _store = store.ThrowIfNull();
        _eventWriter = eventWriter.ThrowIfNull();
        _consumer = consumer.ThrowIfNull();
    }

    protected override async Task ExecutingAsync(CancellationToken cancellationToken)
    {
        await _consumer.ConsumeContinuously(async (message, dbContext) =>
        {
            Logger.LogInformation("Processing {Type}", message.GetType().Name);

            var map = _container.Resolve<EventSourcedEntityMap>();
            var municipalities = new Municipalities(map, _store, SerializerSettings, EventMapping);

            var projector =
                new ConnectedProjector<MunicipalityEventConsumerContext>(
                    Resolve.WhenEqualToHandlerMessageType(new MunicipalityEventProjection(municipalities).Handlers));

            await projector.ProjectAsync(dbContext, message, cancellationToken).ConfigureAwait(false);

            var messageId = Guid.NewGuid(); //TODO-rik van waar messageid halen?

            foreach (var entry in map.Entries)
            {
                var events = entry.Entity.TakeEvents();
                if (events.Any())
                {
                    await _eventWriter.WriteAsync(entry.Stream, entry.ExpectedVersion, messageId, events, cancellationToken);
                }
            }

            //CancellationToken.None to prevent halfway consumption
            await dbContext.SaveChangesAsync(CancellationToken.None);

            Logger.LogInformation("Processed {Type}", message.GetType().Name);
        }, cancellationToken);
    }
}
