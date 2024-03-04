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

public interface IStreetNameSnapshotTopicConsumer
{
    Task ConsumeContinuously(Func<SnapshotMessage, StreetNameSnapshotConsumerContext, Task> messageHandler, CancellationToken cancellationToken);
}

public class StreetNameSnapshotTopicConsumer : IStreetNameSnapshotTopicConsumer
{
    private readonly KafkaOptions _options;
    private readonly IDbContextFactory<StreetNameSnapshotConsumerContext> _dbContextFactory;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger _logger;

    public StreetNameSnapshotTopicConsumer(
        KafkaOptions options,
        IDbContextFactory<StreetNameSnapshotConsumerContext> dbContextFactory,
        ILoggerFactory loggerFactory
    )
    {
        _options = options.ThrowIfNull();
        _dbContextFactory = dbContextFactory.ThrowIfNull();
        _loggerFactory = loggerFactory.ThrowIfNull();
        _logger = loggerFactory.CreateLogger<StreetNameSnapshotTopicConsumer>();
    }

    public async Task ConsumeContinuously(Func<SnapshotMessage, StreetNameSnapshotConsumerContext, Task> messageHandler, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_options.Consumers?.StreetNameSnapshot?.Topic))
        {
            _logger.LogError($"Configuration has no {nameof(KafkaConsumers.StreetNameSnapshot)} Consumer with a Topic.");
            return;
        }

        var kafkaConsumerOptions = _options.Consumers.StreetNameSnapshot;
        var consumerGroupId = $"{nameof(RoadRegistry)}.{nameof(StreetNameSnapshotConsumer)}.{kafkaConsumerOptions.Topic}{kafkaConsumerOptions.GroupSuffix}";

        var jsonSerializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

        var consumerOptions = new ConsumerOptions(
            new BootstrapServers(_options.BootstrapServers),
            new Topic(kafkaConsumerOptions.Topic),
            new ConsumerGroupId(consumerGroupId),
            jsonSerializerSettings,
            new SnapshotMessageSerializer<StreetNameSnapshotRecord>(jsonSerializerSettings)
        );
        if (!string.IsNullOrEmpty(_options.SaslUserName))
        {
            consumerOptions.ConfigureSaslAuthentication(new SaslAuthentication(_options.SaslUserName, _options.SaslPassword));
        }
        if (kafkaConsumerOptions.Offset is not null)
        {
            consumerOptions.ConfigureOffset(new Offset(kafkaConsumerOptions.Offset.Value));
        }

        _logger.LogInformation("Starting to consume Topic '{Topic}' with ConsumerGroupId '{ConsumerGroupId}'", consumerOptions.Topic, consumerOptions.ConsumerGroupId);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await new IdempotentConsumer<StreetNameSnapshotConsumerContext>(consumerOptions, _dbContextFactory, _loggerFactory)
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
