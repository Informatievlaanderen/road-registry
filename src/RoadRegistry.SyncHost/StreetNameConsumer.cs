namespace RoadRegistry.SyncHost;

using Autofac;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Hosts;
using Infrastructure;
using Microsoft.Extensions.Logging;
using RoadRegistry.StreetNameConsumer.Projections;
using RoadRegistry.StreetNameConsumer.Schema;
using StreetName;
using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;

public class StreetNameConsumer : RoadRegistryBackgroundService
{
    private readonly ILifetimeScope _container;
    private readonly KafkaOptions _options;
    private readonly ILoggerFactory _loggerFactory;

    public StreetNameConsumer(
        ILifetimeScope container,
        KafkaOptions options,
        ILoggerFactory loggerFactory)
        : base(loggerFactory.CreateLogger<StreetNameConsumer>())
    {
        _container = container;
        _options = options;
        _loggerFactory = loggerFactory;
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
            var projector = new ConnectedProjector<StreetNameConsumerContext>(Resolve.WhenEqualToHandlerMessageType(new StreetNameConsumerProjection().Handlers));

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

                await new Consumer(consumerOptions, _loggerFactory)
                    .ConsumeContinuously(async message =>
                    {
                        var snapshotMessage = (SnapshotMessage)message;
                        var record = (StreetNameSnapshotOsloRecord)snapshotMessage.Value;

                        Logger.LogInformation("Processing streetname {Key}", snapshotMessage.Key);

                        await using var scope = _container.BeginLifetimeScope();
                        await using var context = scope.Resolve<StreetNameConsumerContext>();

                        await projector.ProjectAsync(context, new StreetNameSnapshotOsloWasProduced
                        {
                            StreetNameId = snapshotMessage.Key,
                            Offset = snapshotMessage.Offset,
                            Record = record
                        }, cancellationToken);

                        await context.SaveChangesAsync(cancellationToken);

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
                await Task.Delay(waitSeconds * 1000, cancellationToken);
            }
        }
    }
}
