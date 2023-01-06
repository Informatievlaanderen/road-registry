namespace RoadRegistry.BackOffice.Infrastructure.Modules;

using Autofac;
using Be.Vlaanderen.Basisregisters.AggregateSource;
using Be.Vlaanderen.Basisregisters.CommandHandling;
using NodaTime;

public class CommandHandlingModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder
            .RegisterType<ConcurrentUnitOfWork>()
            .InstancePerLifetimeScope();

        builder.RegisterInstance<IClock>(SystemClock.Instance);

        //builder
        //    .RegisterEventstreamModule(_configuration);

        //CommandHandlerModules.Register(builder);

        builder
            .RegisterType<CommandHandlerResolver>()
            .As<ICommandHandlerResolver>();
    }
}
