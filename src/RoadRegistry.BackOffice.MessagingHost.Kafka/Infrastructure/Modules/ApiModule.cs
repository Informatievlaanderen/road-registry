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
    using Projections;
    using RoadRegistry.BackOffice;
    using RoadRegistry.BackOffice.MessagingHost.Kafka;

    public class ApiModule : Module
    {
        private readonly IConfiguration _configuration;

        public ApiModule(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var eventSerializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
            
            builder
                .RegisterModule(new DataDogModule(_configuration))
                .RegisterModule<EnvelopeModule>()
                .RegisterModule(new EventHandlingModule(typeof(DomainAssemblyMarker).Assembly, eventSerializerSettings));
        }
    }
}
