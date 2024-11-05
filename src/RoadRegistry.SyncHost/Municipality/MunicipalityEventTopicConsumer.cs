namespace RoadRegistry.SyncHost.Municipality;

using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoadRegistry.Sync.MunicipalityRegistry;
using RoadRegistry.SyncHost.Infrastructure;

public interface IMunicipalityEventTopicConsumer
{
    Task ConsumeContinuously(Func<object, MunicipalityEventConsumerContext, Task> messageHandler, CancellationToken cancellationToken);
}

public class MunicipalityEventTopicConsumer : IMunicipalityEventTopicConsumer
{
    private readonly KafkaOptions _options;
    private readonly IDbContextFactory<MunicipalityEventConsumerContext> _dbContextFactory;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger _logger;

    public MunicipalityEventTopicConsumer(
        KafkaOptions options,
        IDbContextFactory<MunicipalityEventConsumerContext> dbContextFactory,
        ILoggerFactory loggerFactory
    )
    {
        _options = options.ThrowIfNull();
        _dbContextFactory = dbContextFactory.ThrowIfNull();
        _loggerFactory = loggerFactory.ThrowIfNull();
        _logger = loggerFactory.CreateLogger<MunicipalityEventTopicConsumer>();
    }

    public async Task ConsumeContinuously(Func<object, MunicipalityEventConsumerContext, Task> messageHandler, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_options.Consumers?.MunicipalityEvent?.Topic))
        {
            _logger.LogError($"Configuration has no {nameof(KafkaConsumers.MunicipalityEvent)} Consumer with a Topic.");
            return;
        }

        var kafkaConsumerOptions = _options.Consumers.MunicipalityEvent;
        var consumerGroupId = $"{nameof(RoadRegistry)}.{nameof(MunicipalityEventConsumer)}.{kafkaConsumerOptions.Topic}{kafkaConsumerOptions.GroupSuffix}";

        var jsonSerializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

        var consumerOptions = new ConsumerOptions(
            new BootstrapServers(_options.BootstrapServers),
            new Topic(kafkaConsumerOptions.Topic),
            new ConsumerGroupId(consumerGroupId),
            jsonSerializerSettings,
            new GrarContractsMessageSerializer(jsonSerializerSettings)
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
                await new IdempotentConsumer<MunicipalityEventConsumerContext>(consumerOptions, _dbContextFactory, _loggerFactory)
                    .ConsumeContinuously(messageHandler, cancellationToken);
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
