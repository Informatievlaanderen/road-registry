using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RoadRegistry.BackOffice.MessagingHost.Kafka
{
    using Autofac;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Microsoft.Extensions.DependencyInjection;
    using Projections;
    using Resolve = Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector.Resolve;

    public class StreetNameConsumer : BackgroundService
    {
        private readonly ILogger<StreetNameConsumer> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILifetimeScope _container;
        private readonly ILoggerFactory _loggerFactory;
        private readonly KafkaOptions _options;
        private readonly ConsumerOptions _consumerOptions;

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
                            noMessageFoundDelay: 300,
                            offset: null,
                            _options.JsonSerializerSettings),
                        cancellationToken);
                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}
