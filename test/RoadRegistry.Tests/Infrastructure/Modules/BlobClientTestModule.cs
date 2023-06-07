namespace RoadRegistry.Tests.Infrastructure.Modules;

using Autofac;
using RoadRegistry.BackOffice;

public class BlobClientTestModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder
            .RegisterType<MemoryBlobClientFactory>()
            .As<IBlobClientFactory>()
            .SingleInstance();
    }
}