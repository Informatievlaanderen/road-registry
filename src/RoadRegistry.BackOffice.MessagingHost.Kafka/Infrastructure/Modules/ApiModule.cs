namespace RoadRegistry.BackOffice.MessagingHost.Kafka.Infrastructure.Modules
{
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
    using Be.Vlaanderen.Basisregisters.Projector;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Projections;
    using RoadRegistry.BackOffice;
    using RoadRegistry.BackOffice.MessagingHost.Kafka;

    public class ApiModule : Module
    {
        private readonly IConfiguration _configuration;
        private readonly ILoggerFactory _loggerFactory;

        public ApiModule(
            IConfiguration configuration,
            ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _loggerFactory = loggerFactory;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var eventSerializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

            builder
                .RegisterModule(new DataDogModule(_configuration))

                .RegisterModule<EnvelopeModule>()

                .RegisterModule(new EventHandlingModule(typeof(DomainAssemblyMarker).Assembly, eventSerializerSettings));
            
            builder
                .RegisterProjectionMigrator<StreetNameConsumerContextFactory>(
                    _configuration,
                    _loggerFactory)

                .RegisterProjections<StreetNameConsumerProjection, StreetNameConsumerContext>(
                    context => new StreetNameConsumerProjection(),
                    ConnectedProjectionSettings.Default);
        }
    }
}
