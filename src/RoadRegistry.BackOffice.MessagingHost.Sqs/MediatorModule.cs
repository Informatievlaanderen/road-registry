namespace RoadRegistry.BackOffice.MessagingHost.Sqs;

using Autofac;
using MediatR;
using MediatR.Pipeline;

public class MediatorModule : Autofac.Module
{
    private readonly IEnumerable<Type> _typeRegistrations = new[]
    {
        typeof(IRequestHandler<>),
        typeof(IRequestHandler<,>),
        typeof(IRequestExceptionHandler<,,>),
        typeof(IRequestExceptionAction<,>)
    };

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(typeof(IMediator).Assembly).AsImplementedInterfaces();
        builder.RegisterGeneric(typeof(ValidationPipelineBehavior<,>)).As(typeof(IPipelineBehavior<,>));
        builder.Register<ServiceFactory>(ctx =>
        {
            var c = ctx.Resolve<IComponentContext>();
            return t => c.Resolve(t);
        });

        foreach (var mediatrOpenType in _typeRegistrations)
            builder
                .RegisterAssemblyTypes(GetType().Assembly)
                .AsClosedTypesOf(mediatrOpenType)
                .AsImplementedInterfaces();
    }
}
