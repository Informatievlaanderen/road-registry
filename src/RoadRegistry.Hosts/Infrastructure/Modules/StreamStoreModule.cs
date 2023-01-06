namespace RoadRegistry.Hosts.Infrastructure.Modules;

using Autofac;
using Microsoft.Extensions.Configuration;
using SqlStreamStore;

public class StreamStoreModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.Register<IStreamStore>(context =>
                new MsSqlStreamStoreV3(
                    new MsSqlStreamStoreV3Settings(
                            context
                                .Resolve<IConfiguration>()
                                .GetConnectionString(WellknownConnectionNames.Events)
                        )
                        { Schema = WellknownSchemas.EventSchema }))
            .SingleInstance();
    }
}
