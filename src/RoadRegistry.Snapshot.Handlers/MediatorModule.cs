namespace RoadRegistry.Snapshot.Handlers;

using Autofac;
using MediatR;
using MediatR.Pipeline;

public class MediatorModule : Module
{
    private readonly IEnumerable<Type> _typeRegistrations = new[]
    {
        typeof(IRequestHandler<,>),
        typeof(IRequestExceptionHandler<,,>),
        typeof(IRequestExceptionAction<,>)
    };

    protected override void Load(ContainerBuilder builder)
    {
        foreach (var mediatrOpenType in _typeRegistrations)
        {
            builder
                .RegisterAssemblyTypes(GetType().Assembly)
                .AsClosedTypesOf(mediatrOpenType)
                .AsImplementedInterfaces();
        }
    }
}