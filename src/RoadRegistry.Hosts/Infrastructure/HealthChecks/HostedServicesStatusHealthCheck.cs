namespace RoadRegistry.Hosts.Infrastructure.HealthChecks;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal class HostedServicesStatusHealthCheck : IHealthCheck
{
    private readonly HostedServicesStatusHealthCheckOptions _options;

    public HostedServicesStatusHealthCheck(HostedServicesStatusHealthCheckOptions options)
    {
        _options = options;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_options.HostedServices.Any())
            {
                return HealthCheckResult.Unhealthy("No hosted services registered.");
            }

            var hostedServices = _options.HostedServices.Select(x => new
            {
                HostedService = x,
                Status = x is IHostedServiceStatus hostedServiceStatus ? hostedServiceStatus.Status : (HostStatus?)null
            }).ToList();

            var hostedServicesWithStatus = hostedServices
                .Where(x => x.Status is not null)
                .ToList();
            if (hostedServicesWithStatus.Count != hostedServices.Count)
            {
                var hostedServicesWithoutStatus = hostedServices.Where(x => x.Status is null).ToList();
                return HealthCheckResult.Unhealthy($"Services did not implement {nameof(IHostedServiceStatus)}", data: ToDataDictionary(hostedServicesWithoutStatus.Select(x => x.HostedService)));
            }

            var notRunningServices = hostedServicesWithStatus
                .Where(x => x.Status == HostStatus.Stopping || x.Status == HostStatus.Stopped)
                .ToList();
            if (notRunningServices.Any())
            {
                return HealthCheckResult.Unhealthy("Services are not running.", data: ToDataDictionary(notRunningServices.Select(x => x.HostedService)));
            }

            return HealthCheckResult.Healthy(data: ToDataDictionary(hostedServicesWithStatus.Select(x => x.HostedService)));
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }

    private static IReadOnlyDictionary<string, object> ToDataDictionary(IEnumerable<IHostedService> hostedServices)
    {
        return new Dictionary<string, object>
        {
            {
                "services", hostedServices.Select(x => new
                {
                    x.GetType().Name,
                    Status = x is IHostedServiceStatus hostedServiceStatus ? hostedServiceStatus.Status : default
                }).ToList()
            }
        };
    }
}
