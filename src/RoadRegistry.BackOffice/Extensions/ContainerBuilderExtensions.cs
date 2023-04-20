namespace RoadRegistry.BackOffice.Extensions;

using System;
using System.Collections.Generic;
using Autofac;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.Configuration;

public static class ContainerBuilderExtensions
{
    public static ContainerBuilder RegisterModulesFromAssemblyContaining<T>(this ContainerBuilder builder)
    {
        builder.RegisterAssemblyModules(typeof(T).Assembly);
        return builder;
    }

    public static ContainerBuilder RegisterOptions<TOptions>(this ContainerBuilder builder, Action<TOptions> validateOptions = null)
        where TOptions : class, new()
    {
        if (validateOptions != null)
        {
            builder.AddOptionsValidator(validateOptions);
        }

        builder.Register(c => c.Resolve<IConfiguration>().GetOptions<TOptions>()).AsSelf().SingleInstance();
        return builder;
    }

    public static ContainerBuilder RegisterOptions<TOptions>(this ContainerBuilder builder, string configurationSectionKey, Action<TOptions> validateOptions = null)
        where TOptions : class, new()
    {
        if (validateOptions != null)
        {
            builder.AddOptionsValidator(validateOptions);
        }

        builder.Register(c => c.Resolve<IConfiguration>().GetOptions<TOptions>(configurationSectionKey)).AsSelf().SingleInstance();
        return builder;
    }

    private static readonly IEnumerable<Type> MediatrHandlerTypes = new[]
    {
        typeof(IRequestHandler<>),
        typeof(IRequestHandler<,>),
        typeof(IRequestExceptionHandler<,,>),
        typeof(IRequestExceptionAction<,>)
    };

    public static ContainerBuilder RegisterMediatrHandlersFromAssemblyContaining<T>(this ContainerBuilder builder)
    {
        return RegisterMediatrHandlersFromAssemblyContaining(builder, typeof(T));
    }

    public static ContainerBuilder RegisterMediatrHandlersFromAssemblyContaining(this ContainerBuilder builder, Type type)
    {
        foreach (var mediatrOpenType in MediatrHandlerTypes)
            builder
                .RegisterAssemblyTypes(type.Assembly)
                .AsClosedTypesOf(mediatrOpenType)
                .AsImplementedInterfaces();

        return builder;
    }
}
