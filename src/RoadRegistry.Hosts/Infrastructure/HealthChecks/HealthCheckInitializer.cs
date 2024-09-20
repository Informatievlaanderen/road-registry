namespace RoadRegistry.Hosts.Infrastructure.HealthChecks;

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Options;

public class HealthCheckInitializer
{
    private readonly IHealthChecksBuilder _builder;

    private HealthCheckInitializer(IHealthChecksBuilder builder)
    {
        _builder = builder;
    }

    public HealthCheckInitializer AddHostedServicesStatus(Action<HostedServicesStatusHealthCheckOptionsBuilder> setup = null)
    {
        var optionsBuilder = new HostedServicesStatusHealthCheckOptionsBuilder();
        setup?.Invoke(optionsBuilder);

        if (optionsBuilder.IsValid)
        {
            _builder.Add(new HealthCheckRegistration(
                "hosted-services-status".ToLowerInvariant(),
                sp => new HostedServicesStatusHealthCheck(optionsBuilder.With(sp.GetService<IEnumerable<IHostedService>>()
                ).Build()),
                default,
                new[] { "hosts" },
                default));
        }
        return this;
    }
    
    public static HealthCheckInitializer Configure(IHealthChecksBuilder builder)
    {
        return new HealthCheckInitializer(builder);
    }
}
