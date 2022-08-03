namespace RoadRegistry.BackOffice.Handlers;

using Autofac;
using MediatR.Extensions.Autofac.DependencyInjection;

public class MediatRModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterMediatR(typeof(EndpointRequestHandler<,>).Assembly);

        //builder
        //    .RegisterType<Mediator>()
        //    .As<IMediator>()
        //    .InstancePerLifetimeScope();

        //builder.Register<ServiceFactory>(context =>
        //{
        //    var ctx = context.Resolve<IComponentContext>();
        //    return type => ctx.Resolve(type);
        //});

        //builder.RegisterAssemblyTypes(typeof(EndpointRequestHandler<,>).GetTypeInfo().Assembly).AsImplementedInterfaces();
    }
}
