namespace RoadRegistry.SyncHost;

using Autofac;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Hosts;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice;
using RoadRegistry.StreetNameConsumer.Projections;
using RoadRegistry.StreetNameConsumer.Schema;
using StreetName;
using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using BackOffice.Core;
using BackOffice.Framework;
using BackOffice.Messages;
using Newtonsoft.Json;
using SqlStreamStore;

public class StreetNameConsumer : RoadRegistryBackgroundService
{
    private readonly IStreamStore _store;
    private readonly ILifetimeScope _container;
    private readonly IStreetNameEventWriter _streetNameEventWriter;
    private readonly KafkaOptions _options;
    private readonly IDbContextFactory<StreetNameConsumerContext> _dbContextFactory;
    private readonly ILoggerFactory _loggerFactory;

    private static readonly EventMapping EventMapping =
        new(EventMapping.DiscoverEventNamesInAssembly(typeof(StreetNameEvents).Assembly));

    private static readonly JsonSerializerSettings SerializerSettings =
        EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

    public StreetNameConsumer(
        ILifetimeScope container,
        KafkaOptions options,
        IStreamStore store,
        IDbContextFactory<StreetNameConsumerContext> dbContextFactory,
        IStreetNameEventWriter streetNameEventWriter,
        ILoggerFactory loggerFactory)
        : base(loggerFactory.CreateLogger<StreetNameConsumer>())
    {
        _container = container.ThrowIfNull();
        _options = options.ThrowIfNull();
        _store = store.ThrowIfNull();
        _dbContextFactory = dbContextFactory.ThrowIfNull();
        _streetNameEventWriter = streetNameEventWriter.ThrowIfNull();
        _loggerFactory = loggerFactory.ThrowIfNull();
    }

    protected override async Task ExecutingAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_options.Consumers?.StreetName?.Topic))
        {
            Logger.LogError("Configuration has no StreetName Consumer with a Topic.");
            return;
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            //var projector = new ConnectedProjector<StreetNameConsumerContext>(Resolve.WhenEqualToHandlerMessageType(new StreetNameConsumerProjection().Handlers));

            var consumerGroupId = $"{nameof(RoadRegistry)}.{nameof(StreetNameConsumer)}.{_options.Consumers.StreetName.Topic}{_options.Consumers.StreetName.GroupSuffix}";
            try
            {
                var jsonSerializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

                var consumerOptions = new ConsumerOptions(
                        new BootstrapServers(_options.BootstrapServers),
                        new Topic(_options.Consumers.StreetName.Topic),
                        new ConsumerGroupId(consumerGroupId),
                        jsonSerializerSettings,
                        new SnapshotMessageSerializer<StreetNameSnapshotOsloRecord>(jsonSerializerSettings)
                    );
                if (!string.IsNullOrEmpty(_options.SaslUserName))
                {
                    consumerOptions.ConfigureSaslAuthentication(new SaslAuthentication(_options.SaslUserName, _options.SaslPassword));
                }

                await new IdempotentConsumer<StreetNameConsumerContext>(consumerOptions, _dbContextFactory, _loggerFactory)
                    .ConsumeContinuously(async (message, dbContext) =>
                    {
                        var map = _container.Resolve<EventSourcedEntityMap>();
                        var streetNamesContext = new StreetNames(map, _store, SerializerSettings, EventMapping);

                        var snapshotMessage = (SnapshotMessage)message;
                        var record = (StreetNameSnapshotOsloRecord)snapshotMessage.Value;

                        Logger.LogInformation("Processing streetname {Key}", snapshotMessage.Key);

                        //TODO-rik create StreetName eventsourcedentity, use to determine events

                        //TODO-rik split projection naar iets apart, hier enkel events registreren, NIET projecteren, aparte DbContext voorzien voor projectie
                        //await projector.ProjectAsync(dbContext, new StreetNameSnapshotOsloWasProduced
                        //{
                        //    StreetNameId = snapshotMessage.Key,
                        //    Offset = snapshotMessage.Offset,
                        //    Record = record
                        //}, cancellationToken);

                        //_streetNameEventWriter.WriteAsync()

                        //TODO-rik produce internal streetname events (zoals OR sync), of commands om er iets mee te kunnen doen
                        //StreetNameCreated (event)
                        //StreetNameModified (event), enkel Name, HomoniemeToevoeging, of Status kan wijzigen
                        //StreetNameRemoved (event)

                        //TODO-rik (koen) + ChangeRoadNetwork (command) bij delete

                        await dbContext.SaveChangesAsync(cancellationToken);

                        Logger.LogInformation("Processed streetname {Key}", snapshotMessage.Key);
                    }, cancellationToken);
            }
            catch (ConfigurationErrorsException ex)
            {
                Logger.LogError(ex.Message);
                return;
            }
            catch (Exception ex)
            {
                const int waitSeconds = 30;
                Logger.LogCritical(ex, "Error consuming kafka events, trying again in {seconds} seconds", waitSeconds);
                await Task.Delay(TimeSpan.FromSeconds(waitSeconds), cancellationToken);
            }
        }
    }
}
