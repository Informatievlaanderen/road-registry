namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Infrastructure.Modules;

using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac;
using BackOffice;
using MediatR;
using MediatR.Pipeline;
using Module = Autofac.Module;

public class MediatorModule : Module
{
    private readonly IEnumerable<Type> _mediatorOpenTypes = new[]
    {
        typeof(IRequestHandler<,>),
        typeof(IRequestExceptionHandler<,,>),
        typeof(IRequestExceptionAction<,>),
        typeof(INotificationHandler<>),
        typeof(IStreamRequestHandler<,>)
    };

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(typeof(IMediator).GetTypeInfo().Assembly).AsImplementedInterfaces();
        builder.RegisterGeneric(typeof(ValidationPipelineBehavior<,>)).As(typeof(IPipelineBehavior<,>));

        foreach (var mediatrOpenType in _mediatorOpenTypes)
        {
            builder
                .RegisterAssemblyTypes(GetType().Assembly)
                .AsClosedTypesOf(mediatrOpenType)
                .AsImplementedInterfaces();
        }
    }
}
