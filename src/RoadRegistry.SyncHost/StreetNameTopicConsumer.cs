namespace RoadRegistry.SyncHost;

using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sync.StreetNameRegistry;
using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using StreetName;

public interface IStreetNameTopicConsumer
{
    Task ConsumeContinuously(Func<SnapshotMessage, StreetNameConsumerContext, Task> messageHandler, CancellationToken cancellationToken);
}

public class StreetNameTopicConsumer : IStreetNameTopicConsumer
{
    private readonly KafkaOptions _options;
    private readonly IDbContextFactory<StreetNameConsumerContext> _dbContextFactory;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger _logger;

    public StreetNameTopicConsumer(
        KafkaOptions options,
        IDbContextFactory<StreetNameConsumerContext> dbContextFactory,
        ILoggerFactory loggerFactory
    )
    {
        _options = options.ThrowIfNull();
        _dbContextFactory = dbContextFactory.ThrowIfNull();
        _loggerFactory = loggerFactory.ThrowIfNull();
        _logger = loggerFactory.CreateLogger<StreetNameTopicConsumer>();
    }

    public async Task ConsumeContinuously(Func<SnapshotMessage, StreetNameConsumerContext, Task> messageHandler, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_options.Consumers?.StreetName?.Topic))
        {
            _logger.LogError("Configuration has no StreetName Consumer with a Topic.");
            return;
        }

        var consumerGroupId = $"{nameof(RoadRegistry)}.{nameof(StreetNameConsumer)}.{_options.Consumers.StreetName.Topic}{_options.Consumers.StreetName.GroupSuffix}";

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

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await new IdempotentConsumer<StreetNameConsumerContext>(consumerOptions, _dbContextFactory, _loggerFactory)
                    .ConsumeContinuously((message, dbContext) => messageHandler((SnapshotMessage)message, dbContext), cancellationToken);
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
                await Task.Delay(TimeSpan.FromSeconds(waitSeconds), cancellationToken);
            }
        }
    }
}
