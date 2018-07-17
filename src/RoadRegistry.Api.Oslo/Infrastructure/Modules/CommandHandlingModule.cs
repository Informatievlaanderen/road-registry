namespace RoadRegistry.Api.Oslo.Infrastructure.Modules
{
    using Aiv.Vbr.AggregateSource;
    using Aiv.Vbr.AggregateSource.SqlStreamStore.Autofac;
    using Aiv.Vbr.CommandHandling;
    using Autofac;
    using RoadRegistry.Infrastructure;
    using Microsoft.Extensions.Configuration;

    public class CommandHandlingModule : Module
    {
        private readonly IConfiguration _configuration;

        public CommandHandlingModule(IConfiguration configuration)
            => _configuration = configuration;

        protected override void Load(ContainerBuilder containerBuilder)
        {
            containerBuilder
                .RegisterModule<RepositoriesModule>();

            containerBuilder
                .RegisterType<ConcurrentUnitOfWork>()
                .InstancePerLifetimeScope();

            containerBuilder
                .RegisterModule(new SqlStreamStoreModule(_configuration.GetConnectionString("Events"), Schema.Events));

            CommandHandlerModules.Register(containerBuilder);

            containerBuilder
                .RegisterType<CommandHandlerResolver>()
                .As<ICommandHandlerResolver>();
        }
    }
}
