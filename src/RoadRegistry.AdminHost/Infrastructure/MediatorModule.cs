namespace RoadRegistry.AdminHost.Infrastructure;

using Autofac;
using BackOffice;
using MediatR;

public class MediatorModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(typeof(IMediator).Assembly).AsImplementedInterfaces();
        builder.RegisterGeneric(typeof(ValidationPipelineBehavior<,>)).As(typeof(IPipelineBehavior<,>));

        builder.RegisterModule(new BackOffice.Handlers.MediatorModule());
    }
}
