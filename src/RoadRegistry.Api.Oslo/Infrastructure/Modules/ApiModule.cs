namespace RoadRegistry.Api.Oslo.Infrastructure.Modules
{
    using Aiv.Vbr.CommandHandling.Idempotency;
    using Aiv.Vbr.EventHandling;
    using Aiv.Vbr.EventHandling.Autofac;
    using Aiv.Vbr.ProjectionHandling.SqlStreamStore.Autofac;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using RoadRegistry.Infrastructure;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Projections.Oslo;

    public class ApiModule : Module
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceCollection _services;
        private readonly ILoggerFactory _loggerFactory;

        public ApiModule(
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
                .RegisterModule(new IdempotencyModule(
                    _services,
                    _configuration.GetSection(IdempotencyConfiguration.Section).Get<IdempotencyConfiguration>().ConnectionString,
                    new IdempotencyMigrationsTableInfo(Schema.Events),
                    new IdempotencyTableInfo(Schema.Events),
                    _loggerFactory));

            containerBuilder
                .RegisterModule(new OsloModule(_configuration, _services, _loggerFactory));

            containerBuilder
                .RegisterModule(new EventHandlingModule(typeof(DomainAssemblyMarker).Assembly, eventSerializerSettings));

            containerBuilder
                .RegisterModule(new EnvelopeModule());

            containerBuilder
                .RegisterModule(new CommandHandlingModule(_configuration));

            containerBuilder.Populate(_services);
        }
    }
}
