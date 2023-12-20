namespace RoadRegistry.Hosts.Infrastructure.Options;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Hosting;

public class HostedServicesStatusHealthCheckOptionsBuilder : HealthCheckOptionsBuilder<HostedServicesStatusHealthCheckOptions>
{
    private readonly List<string> _excludeHostedServiceTypeFullNames = new();
    private ICollection<IHostedService> _hostedServices;

    public override bool IsValid => true;

    public HostedServicesStatusHealthCheckOptionsBuilder()
    {
        ExcludeHostedService("Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckPublisherHostedService");
        ExcludeHostedService("Microsoft.AspNetCore.Hosting.GenericWebHostService");
    }

    public override HostedServicesStatusHealthCheckOptions Build()
    {
        return new HostedServicesStatusHealthCheckOptions
        {
            HostedServices = _hostedServices ?? Array.Empty<IHostedService>()
        };
    }

    public HostedServicesStatusHealthCheckOptionsBuilder ExcludeHostedService<T>()
        where T : IHostedService
    {
        return ExcludeHostedService(typeof(T).FullName);
    }

    public HostedServicesStatusHealthCheckOptionsBuilder ExcludeHostedService(string typeFullName)
    {
        if (_excludeHostedServiceTypeFullNames.Contains(typeFullName))
        {
            throw new InvalidOperationException($"Type '{typeFullName}' is already registered.");
        }

        _excludeHostedServiceTypeFullNames.Add(typeFullName);
        return this;
    }

    public HostedServicesStatusHealthCheckOptionsBuilder With(IEnumerable<IHostedService> hostedServices)
    {
        _hostedServices = hostedServices
            .Where(x => !_excludeHostedServiceTypeFullNames.Contains(x.GetType().FullName))
            .ToArray();
        return this;
    }
}
