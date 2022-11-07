namespace RoadRegistry.BackOffice.MessagingHost.Kafka;

using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Handlers.Kafka;
using Handlers.Kafka.Projections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class StreetNameConsumer : BackgroundService
{
    private readonly ConsumerOptions _consumerOptions;
    private readonly ILogger<StreetNameConsumer> _logger;
    private readonly KafkaOptions _options;
    private readonly IServiceProvider _serviceProvider;

    public StreetNameConsumer(
        ILifetimeScope container,
        ILoggerFactory loggerFactory,
        KafkaOptions options,
        ConsumerOptions consumerOptions,
        ILogger<StreetNameConsumer> logger,
        IServiceProvider serviceProvider)
    {
        _options = options;
        _consumerOptions = consumerOptions;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!stoppingToken.IsCancellationRequested)
        {
            var projector = new ConnectedProjector<StreetNameConsumerContext>(Resolve.WhenEqualToHandlerMessageType(new StreetNameConsumerProjection().Handlers));

            var consumerGroupId = $"{nameof(RoadRegistry)}.{nameof(StreetNameConsumer)}.{_consumerOptions.Topic}{_consumerOptions.ConsumerGroupSuffix}";
            try
            {
                await KafkaConsumer.Consume(
                    new KafkaConsumerOptions(
                        _options.BootstrapServers,
                        _options.SaslUserName,
                        _options.SaslPassword,
                        consumerGroupId,
                        _consumerOptions.Topic,
                        async message =>
                        {
                            using var scope = _serviceProvider.CreateScope();
                            await using var context = scope.ServiceProvider.GetRequiredService<StreetNameConsumerContext>();
                            await projector.ProjectAsync(context, message, stoppingToken);
                        },
                        300,
                        null,
                        _options.JsonSerializerSettings),
                    stoppingToken);
            }
            catch (Exception ex)
            {
                const int waitSeconds = 30;
                _logger.LogCritical(ex, "Error consuming kafka events, trying again in {seconds} seconds", waitSeconds);
                await Task.Delay(waitSeconds * 1000, stoppingToken);
            }
        }
    }
}
