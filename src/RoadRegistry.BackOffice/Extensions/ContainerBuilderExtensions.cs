namespace RoadRegistry.BackOffice.Extensions;

using Autofac;
using Autofac.Core.Registration;

public static class ContainerBuilderExtensions
{
    public static IModuleRegistrar RegisterModulesFromAssemblyContaining<T>(this ContainerBuilder builder)
    {
        return builder.RegisterAssemblyModules(typeof(T).Assembly);
    }
}
