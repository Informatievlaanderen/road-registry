namespace RoadRegistry.SyncHost;

using Autofac;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Infrastructure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RoadRegistry.StreetNameConsumer.Projections;
using RoadRegistry.StreetNameConsumer.Schema;
using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using StreetName;

public class StreetNameConsumer : BackgroundService
{
    private readonly ILifetimeScope _container;
    private readonly KafkaOptions _options;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<StreetNameConsumer> _logger;

    public StreetNameConsumer(
        ILifetimeScope container,
        KafkaOptions options,
        ILoggerFactory loggerFactory)
    {
        _container = container;
        _options = options;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<StreetNameConsumer>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrEmpty(_options.Consumers?.StreetName?.Topic))
        {
            _logger.LogError("Configuration has no StreetName Consumer with a Topic.");
            return;
        }

        _logger.LogInformation("Consuming streetnames started...");
        try
        {
            while (!stoppingToken.IsCancellationRequested)
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
                            
                            _logger.LogInformation("Processing streetname {Key}", snapshotMessage.Key);

                            await using var scope = _container.BeginLifetimeScope();
                            await using var context = scope.Resolve<StreetNameConsumerContext>();

                            await projector.ProjectAsync(context, new StreetNameSnapshotOsloWasProduced
                            {
                                StreetNameId = snapshotMessage.Key,
                                Offset = snapshotMessage.Offset,
                                Record = record
                            }, stoppingToken);

                            await context.SaveChangesAsync(stoppingToken);

                            _logger.LogInformation("Processed streetname {Key}", snapshotMessage.Key);
                        }, stoppingToken);
                }
                catch (ConfigurationErrorsException ex)
                {
                    _logger.LogError(ex.Message);
                    return;
                }
                catch (Exception ex)
                {
                    const int waitSeconds = 30;
                    _logger.LogCritical(ex, "Error consuming kafka events, trying again in {seconds} seconds", waitSeconds);
                    await Task.Delay(waitSeconds * 1000, stoppingToken);
                }
            }
        }
        finally
        {
            _logger.LogInformation("Consuming streetnames stopped.");
        }
    }
}
