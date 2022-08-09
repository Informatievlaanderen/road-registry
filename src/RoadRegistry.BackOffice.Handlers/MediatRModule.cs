namespace RoadRegistry.BackOffice.Handlers;

using Autofac;
using MediatR;

public class MediatRModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder
            .RegisterType<Mediator>()
            .As<IMediator>()
            .InstancePerLifetimeScope();

        builder.Register<ServiceFactory>(context =>
        {
            var ctx = context.Resolve<IComponentContext>();
            return type => ctx.Resolve(type);
        });

        builder.RegisterAssemblyTypes(typeof(MediatRModule).Assembly).AsImplementedInterfaces();
    }
}
