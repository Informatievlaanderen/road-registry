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

    public StreetNameTopicConsumer(
        KafkaOptions options,
        IDbContextFactory<StreetNameConsumerContext> dbContextFactory,
        ILoggerFactory loggerFactory
    )
    {
        _options = options;
        _dbContextFactory = dbContextFactory;
        _loggerFactory = loggerFactory;
    }

    public Task ConsumeContinuously(Func<SnapshotMessage, StreetNameConsumerContext, Task> messageHandler, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_options.Consumers?.StreetName?.Topic))
        {
            throw new ConfigurationErrorsException("Configuration has no StreetName Consumer with a Topic.");
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

        return new IdempotentConsumer<StreetNameConsumerContext>(consumerOptions, _dbContextFactory, _loggerFactory)
            .ConsumeContinuously((message, dbContext) => messageHandler((SnapshotMessage)message, dbContext), cancellationToken);
    }
}
