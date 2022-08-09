namespace RoadRegistry.BackOffice.Sqs.Handlers;

using Autofac;

public class MediatRModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(typeof(MediatRModule).Assembly).AsImplementedInterfaces();
    }
}
