namespace RoadRegistry.BackOffice.Extensions;

using System;
using Autofac;
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
}
