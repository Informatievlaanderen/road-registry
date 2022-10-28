namespace RoadRegistry.BackOffice.MessagingHost.Kafka;

using Autofac;
using MediatR;

public class MediatorModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(typeof(IMediator).Assembly).AsImplementedInterfaces();
        builder.RegisterGeneric(typeof(ValidationPipelineBehavior<,>)).As(typeof(IPipelineBehavior<,>));

        builder.Register<ServiceFactory>(ctx =>
        {
            var c = ctx.Resolve<IComponentContext>();
            return t => c.Resolve(t);
        });

        builder.RegisterModule(new Handlers.MediatorModule());
        builder.RegisterModule(new Handlers.Kafka.MediatorModule());
    }
}
