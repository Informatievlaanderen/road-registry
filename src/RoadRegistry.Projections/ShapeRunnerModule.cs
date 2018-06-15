namespace RoadRegistry.Projections
{
    using Aiv.Vbr.AggregateSource.SqlStreamStore.Autofac;
    using Aiv.Vbr.EventHandling;
    using Aiv.Vbr.EventHandling.Autofac;
    using Aiv.Vbr.ProjectionHandling.SqlStreamStore.Autofac;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using GeoAPI.Geometries;
    using Infrastructure;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NetTopologySuite;
    using NetTopologySuite.IO;

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
                .RegisterModule(new EventHandlingModule(typeof(DomainAssemblyMarker).Assembly, eventSerializerSettings));

            containerBuilder
                .RegisterModule(new EnvelopeModule());

            containerBuilder
                .RegisterModule(new SqlStreamStoreModule(_configuration.GetConnectionString("Events"), Schema.Default));

            containerBuilder.RegisterType<RoadShapeRunner>()
                .SingleInstance();

            containerBuilder.RegisterInstance<WKBReader>(new WKBReader(new NtsGeometryServices())
            {
                HandleOrdinates = Ordinates.XYZM,
                HandleSRID = true
            });

            containerBuilder.Populate(_services);
        }
    }
}
