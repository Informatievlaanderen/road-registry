//namespace RoadRegistry.Api.Modules
//{
//    using Be.Vlaanderen.Basisregisters.AggregateSource;
//    using Be.Vlaanderen.Basisregisters.CommandHandling;
//    using Autofac;
//    using BackOfficeSchema;
//    using Microsoft.Extensions.Configuration;
//
//    public class CommandHandlingModule : Module
//    {
//        private readonly IConfiguration _configuration;
//
//        public CommandHandlingModule(IConfiguration configuration)
//            => _configuration = configuration;
//
//        protected override void Load(ContainerBuilder containerBuilder)
//        {
//            containerBuilder
//                .RegisterModule<RepositoriesModule>();
//
//            containerBuilder
//                .RegisterType<ConcurrentUnitOfWork>()
//                .InstancePerLifetimeScope();
//
//            containerBuilder
//                .RegisterModule(new SqlStreamStoreModule(_configuration.GetConnectionString("Events"), Schema.Events));
//
//            CommandHandlerModules.Register(containerBuilder);
//
//            containerBuilder
//                .RegisterType<CommandHandlerResolver>()
//                .As<ICommandHandlerResolver>();
//        }
//    }
//}
