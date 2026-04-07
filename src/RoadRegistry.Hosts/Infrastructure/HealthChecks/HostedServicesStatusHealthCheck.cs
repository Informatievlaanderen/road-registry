namespace RoadRegistry.Hosts.Infrastructure.HealthChecks;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

internal class HostedServicesStatusHealthCheck : IHealthCheck
{
    private readonly ICollection<IHostedService> _hostedServices;

    public HostedServicesStatusHealthCheck(HostedServicesStatusHealthCheckOptions options, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger(GetType());

        var hostedServices = options.HostedServices.Select(x => new
        {
            HostedService = x,
            Status = x is IHostedServiceStatus hostedServiceStatus ? hostedServiceStatus.Status : (HostStatus?)null
        }).ToArray();

        foreach (var service in hostedServices.Where(x => x.Status is null))
        {
            logger.LogWarning("Service {ServiceType} did not implement {InterfaceType} and thus is not registered in the health check.", service!.HostedService.GetType().FullName, nameof(IHostedServiceStatus));
        }

        _hostedServices = hostedServices
            .Where(x => x.Status is not null)
            .Select(x => x.HostedService)
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

            var hostedServices = _hostedServices.Select(x => new
            {
                HostedService = x,
                ((IHostedServiceStatus)x).Status
            }).ToList();

            var notRunningServices = hostedServices
                .Where(x => x.Status == HostStatus.Stopping || x.Status == HostStatus.Stopped)
                .ToList();
            if (notRunningServices.Any())
            {
                return HealthCheckResult.Unhealthy("Services are not running.", data: ToDataDictionary(notRunningServices.Select(x => x.HostedService)));
            }

            return HealthCheckResult.Healthy(data: ToDataDictionary(_hostedServices));
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
