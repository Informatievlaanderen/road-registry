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
    private readonly HostedServicesStatusHealthCheckOptions _options;
    private readonly ILogger _logger;

    public HostedServicesStatusHealthCheck(HostedServicesStatusHealthCheckOptions options, ILoggerFactory loggerFactory)
    {
        _options = options;
        _logger = loggerFactory.CreateLogger(GetType());
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(CheckHealth(context));
    }
    private HealthCheckResult CheckHealth(HealthCheckContext context)
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

            var hostedServicesWithoutStatus = hostedServices.Where(x => x.Status is null).ToList();
            foreach (var service in hostedServicesWithoutStatus)
            {
                _logger.LogWarning($"Service {service.HostedService.GetType().FullName} did not implement {nameof(IHostedServiceStatus)} and thus is not registered in the health check.");
            }

            var hostedServicesWithStatus = hostedServices
                .Where(x => x.Status is not null)
                .ToList();
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
