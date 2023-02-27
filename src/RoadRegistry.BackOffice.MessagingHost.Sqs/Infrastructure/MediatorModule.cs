namespace RoadRegistry.BackOffice.MessagingHost.Sqs.Infrastructure;

using Autofac;
using MediatR;

public class MediatorModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(typeof(IMediator).Assembly).AsImplementedInterfaces();
        builder.RegisterGeneric(typeof(ValidationPipelineBehavior<,>)).As(typeof(IPipelineBehavior<,>));

        builder.RegisterModule(new Handlers.MediatorModule());
    }
}
