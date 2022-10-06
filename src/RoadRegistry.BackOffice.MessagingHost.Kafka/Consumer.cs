using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using RoadRegistry.BackOffice.Framework;
using Resolve = RoadRegistry.BackOffice.Framework.Resolve;

namespace RoadRegistry.BackOffice.MessagingHost.Kafka
{
    using Autofac;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Framework;
    using Resolve = Framework.Resolve;

    public class Consumer : BackgroundService
    {
        private readonly ILogger<Consumer> _logger;
        private readonly ILifetimeScope _container;
        private readonly ILoggerFactory _loggerFactory;
        //private readonly KafkaOptions _options;
        //private readonly ConsumerOptions _consumerOptions;

        public Consumer(
            ILifetimeScope container,
            ILoggerFactory loggerFactory,
            //KafkaOptions options,
            //ConsumerOptions consumerOptions,
            ILogger<Consumer> logger)
        {
            _container = container;
            _loggerFactory = loggerFactory;
            //_options = options;
            //_consumerOptions = consumerOptions;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            //var commandHandler = new CommandHandler(_container, _loggerFactory.CreateLogger<CommandHandler>());
            //var projector = new ConnectedProjector<CommandHandler>(Resolve.WhenEqualToHandlerMessageType(new MunicipalityKafkaProjection().Handlers));

            //var consumerGroupId = $"{nameof(StreetNameRegistry)}.{nameof(Consumer)}.{_consumerOptions.Topic}{_consumerOptions.ConsumerGroupSuffix}";
            //return KafkaConsumer.Consume(
            //    new KafkaConsumerOptions(
            //        _options.BootstrapServers,
            //        _options.SaslUserName,
            //        _options.SaslPassword,
            //        consumerGroupId,
            //        _consumerOptions.Topic,
            //        async message =>
            //        {
            //            await projector.ProjectAsync(commandHandler, message, cancellationToken);
            //        },
            //        noMessageFoundDelay: 300,
            //        offset: null,
            //        _options.JsonSerializerSettings),
            //    cancellationToken);
        }
    }
}
