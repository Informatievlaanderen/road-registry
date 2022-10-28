namespace RoadRegistry.BackOffice.MessagingHost.Kafka;

using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Projections;

public class StreetNameConsumer : BackgroundService
{
    private readonly ConsumerOptions _consumerOptions;
    private readonly ILifetimeScope _container;
    private readonly ILogger<StreetNameConsumer> _logger;
    private readonly ILoggerFactory _loggerFactory;
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
        _container = container;
        _loggerFactory = loggerFactory;
        _options = options;
        _consumerOptions = consumerOptions;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
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
                            using (var scope = _serviceProvider.CreateScope())
                            using (var context = scope.ServiceProvider.GetRequiredService<StreetNameConsumerContext>())
                            {
                                await projector.ProjectAsync(context, message, cancellationToken);
                            }
                        },
                        300,
                        null,
                        _options.JsonSerializerSettings),
                    cancellationToken);
            }
            catch (Exception ex)
            {
            }
        }
    }
}