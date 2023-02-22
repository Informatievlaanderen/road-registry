namespace RoadRegistry.BackOffice.Extensions;

using Autofac;
using Autofac.Core.Registration;
using Microsoft.Extensions.Configuration;

public static class ContainerBuilderExtensions
{
    public static IModuleRegistrar RegisterModulesFromAssemblyContaining<T>(this ContainerBuilder builder)
    {
        return builder.RegisterAssemblyModules(typeof(T).Assembly);
    }

    public static ContainerBuilder RegisterOptions<TOptions>(this ContainerBuilder builder)
        where TOptions : class, new()
    {
        builder.Register(c => c.Resolve<IConfiguration>().GetOptions<TOptions>()).AsSelf().SingleInstance();
        return builder;
    }

    public static ContainerBuilder RegisterOptions<TOptions>(this ContainerBuilder builder, string configurationSectionKey)
        where TOptions : class, new()
    {
        builder.Register(c => c.Resolve<IConfiguration>().GetOptions<TOptions>(configurationSectionKey)).AsSelf().SingleInstance();
        return builder;
    }
}
