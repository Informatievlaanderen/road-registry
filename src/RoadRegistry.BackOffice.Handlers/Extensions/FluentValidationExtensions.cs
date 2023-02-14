namespace RoadRegistry.BackOffice.Handlers.Extensions;

using System.Linq;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

public static class FluentValidationExtensions
{
    public static IServiceCollection AddValidatorsAsScopedFromAssemblyContaining<T>(this IServiceCollection services)
    {
        var validatorTypes = typeof(T).Assembly
            .GetTypes()
            .Where(type => typeof(IValidator).IsAssignableFrom(type) && !type.IsAbstract)
            .ToArray();

        foreach (var validatorType in validatorTypes)
        {
            services.AddScoped(validatorType);
        }

        return services;
    }
}
