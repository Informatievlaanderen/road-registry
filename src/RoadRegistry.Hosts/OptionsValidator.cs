namespace RoadRegistry.Hosts;

using System;
using System.Linq;
using Autofac;
using Autofac.Core;
using BackOffice;
using Microsoft.Extensions.DependencyInjection;

public class OptionsValidator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILifetimeScope _scope;

    public OptionsValidator(
        IServiceProvider serviceProvider,
        ILifetimeScope scope
    )
    {
        _serviceProvider = serviceProvider;
        _scope = scope;
    }

    public void ValidateAndThrow()
    {
        var types = _scope.ComponentRegistry.Registrations
            .SelectMany(e => e.Services)
            .OfType<TypedService>()
            .Where(s => typeof(IOptionsValidator).IsAssignableFrom(s.ServiceType))
            .Select(x => x.ServiceType)
            .Distinct()
            .ToList();

        foreach (var type in types)
        {
            var optionsValidator = (IOptionsValidator)_serviceProvider.GetRequiredService(type);
            var optionsType = optionsValidator.GetOptionsType();
            var options = _serviceProvider.GetRequiredService(optionsType);

            optionsValidator.ValidateAndThrow(options);
        }
    }
}

