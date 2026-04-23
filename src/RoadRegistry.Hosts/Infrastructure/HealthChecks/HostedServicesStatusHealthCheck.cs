namespace RoadRegistry.Hosts.Infrastructure.HealthChecks;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RoadRegistry.Hosts.Infrastructure.Options;

internal class HostedServicesStatusHealthCheck : IHealthCheck
{
    private readonly ICollection<IHostedServiceStatus> _hostedServices;

    public HostedServicesStatusHealthCheck(HostedServicesStatusHealthCheckOptions options)
    {
        _hostedServices = options.HostedServices
            .OfType<IHostedServiceStatus>()
            .ToArray();
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(CheckHealth(context));
    }
    private HealthCheckResult CheckHealth(HealthCheckContext context)
    {
        try
        {
            if (!_hostedServices.Any())
            {
                return HealthCheckResult.Unhealthy("No hosted services registered.");
            }

            var notRunningServices = _hostedServices
                .Where(x => x.Status == HostStatus.Stopping || x.Status == HostStatus.Stopped)
                .ToList();
            if (notRunningServices.Any())
            {
                return HealthCheckResult.Unhealthy("Services are not running.", data: ToDataDictionary(notRunningServices));
            }

            return HealthCheckResult.Healthy(data: ToDataDictionary(_hostedServices));
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }

    private static IReadOnlyDictionary<string, object> ToDataDictionary(IEnumerable<IHostedServiceStatus> hostedServices)
    {
        return new Dictionary<string, object>
        {
            {
                "services", hostedServices.Select(x => new
                {
                    x.GetType().Name,
                    x.Status
                }).ToList()
            }
        };
    }
}
