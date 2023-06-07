namespace RoadRegistry.BackOffice.Api.Infrastructure.Modules;

using Autofac;

public class ApiModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder
            .RegisterType<IfMatchHeaderValidator>()
            .As<IIfMatchHeaderValidator>()
            .AsSelf();
    }
}