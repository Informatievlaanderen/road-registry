namespace RoadRegistry.BackOffice.Extensions;

using System;
using Autofac;
using Microsoft.Extensions.DependencyInjection;

public static class OptionsValidatorExtensions
{
    public static IServiceCollection AddOptionsValidator<TOptions>(this IServiceCollection services, Action<TOptions> validateAndThrow)
    {
        return services
            .AddSingleton(new GenericOptionsValidator<TOptions>(validateAndThrow));
    }

    public static ContainerBuilder AddOptionsValidator<TOptions>(this ContainerBuilder builder, Action<TOptions> validateAndThrow)
    {
        builder
            .RegisterInstance(new GenericOptionsValidator<TOptions>(validateAndThrow));
        return builder;
    }
}
