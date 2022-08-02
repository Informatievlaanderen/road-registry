namespace RoadRegistry.BackOffice.Handlers
{
    using Autofac;
    using MediatR;
    using System.Reflection;

    public class MediatRModule : Autofac.Module
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

            builder.RegisterAssemblyTypes(typeof(EndpointRequestHandler<,>).GetTypeInfo().Assembly).AsImplementedInterfaces();
        }
    }
}
