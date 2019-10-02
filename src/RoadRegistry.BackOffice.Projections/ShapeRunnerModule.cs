namespace RoadRegistry.BackOffice.Projections
{
    using Be.Vlaanderen.Basisregisters.AggregateSource.SqlStreamStore.Autofac;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
    using Messages;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Schema;

    public class ShapeRunnerModule : Module
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceCollection _services;
        private readonly ILoggerFactory _loggerFactory;

        public ShapeRunnerModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _services = services;
            _loggerFactory = loggerFactory;
        }

        protected override void Load(ContainerBuilder containerBuilder)
        {
            var eventSerializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

            containerBuilder
                .RegisterModule(new ShapeModule(_configuration, _services, _loggerFactory));

            containerBuilder
                .RegisterModule(new EventHandlingModule(typeof(RoadNetworkEvents).Assembly, eventSerializerSettings));

            containerBuilder
                .RegisterModule(new EnvelopeModule());

            containerBuilder
                .RegisterModule(new SqlStreamStoreModule(_configuration.GetConnectionString("Events"), Schema.Events));

            containerBuilder.RegisterType<RoadShapeRunner>()
                .SingleInstance();

            containerBuilder.RegisterType<WellKnownBinaryReader>()
                .SingleInstance();

            containerBuilder.Populate(_services);
        }
    }
}
