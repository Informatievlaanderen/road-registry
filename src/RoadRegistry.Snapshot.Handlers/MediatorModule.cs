namespace RoadRegistry.Snapshot.Handlers;

using Autofac;
using BackOffice.Extensions;

public class MediatorModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterMediatrHandlersFromAssemblyContaining(GetType());
    }
}
