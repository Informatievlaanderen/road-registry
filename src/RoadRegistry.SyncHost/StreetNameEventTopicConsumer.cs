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

public interface IStreetNameEventTopicConsumer
{
    Task ConsumeContinuously(Func<object, StreetNameEventConsumerContext, Task> messageHandler, CancellationToken cancellationToken);
}

public class StreetNameEventTopicConsumer : IStreetNameEventTopicConsumer
{
    private readonly KafkaOptions _options;
    private readonly IDbContextFactory<StreetNameEventConsumerContext> _dbContextFactory;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger _logger;

    public StreetNameEventTopicConsumer(
        KafkaOptions options,
        IDbContextFactory<StreetNameEventConsumerContext> dbContextFactory,
        ILoggerFactory loggerFactory
    )
    {
        _options = options.ThrowIfNull();
        _dbContextFactory = dbContextFactory.ThrowIfNull();
        _loggerFactory = loggerFactory.ThrowIfNull();
        _logger = loggerFactory.CreateLogger<StreetNameEventTopicConsumer>();
    }

    public async Task ConsumeContinuously(Func<object, StreetNameEventConsumerContext, Task> messageHandler, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_options.Consumers?.StreetNameEvent?.Topic))
        {
            _logger.LogError($"Configuration has no {nameof(KafkaConsumers.StreetNameEvent)} Consumer with a Topic.");
            return;
        }

        var consumerGroupId = $"{nameof(RoadRegistry)}.{nameof(StreetNameEventConsumer)}.{_options.Consumers.StreetNameEvent.Topic}{_options.Consumers.StreetNameEvent.GroupSuffix}";

        var jsonSerializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

        var consumerOptions = new ConsumerOptions(
            new BootstrapServers(_options.BootstrapServers),
            new Topic(_options.Consumers.StreetNameEvent.Topic),
            new ConsumerGroupId(consumerGroupId),
            jsonSerializerSettings,
            new GrarContractsMessageSerializer(jsonSerializerSettings)
        );
        if (!string.IsNullOrEmpty(_options.SaslUserName))
        {
            consumerOptions.ConfigureSaslAuthentication(new SaslAuthentication(_options.SaslUserName, _options.SaslPassword));
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await new IdempotentConsumer<StreetNameEventConsumerContext>(consumerOptions, _dbContextFactory, _loggerFactory)
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
