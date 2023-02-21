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
        var configurationSectionName = new TOptions() is IHasConfigurationKey hasConfigurationKey ? hasConfigurationKey.GetConfigurationKey() : null;
        return RegisterOptions<TOptions>(builder, configurationSectionName);
    }

    public static ContainerBuilder RegisterOptions<TOptions>(this ContainerBuilder builder, string configurationSectionKey)
        where TOptions : class, new()
    {
        builder.Register(c =>
        {
            var configuration = c.Resolve<IConfiguration>();
            var options = new TOptions();

            if (configurationSectionKey != null)
            {
                configuration.GetSection(configurationSectionKey).Bind(options);
            }
            else
            {
                configuration.Bind(options);
            }

            return options;
        }).AsSelf().SingleInstance();
        return builder;
    }
}
