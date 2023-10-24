namespace RoadRegistry.Hosts.Infrastructure.HealthChecks;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Options;
using System;
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

            var hostedServicesWithStatus = _options.HostedServices.OfType<IHostedServiceStatus>().ToList();
            if (hostedServicesWithStatus.Count != _options.HostedServices.Count)
            {
                var hostedServicesWithoutStatus = _options.HostedServices.Where(x => !(x is IHostedServiceStatus)).ToList();
                return HealthCheckResult.Unhealthy($"Services did not implement {nameof(IHostedServiceStatus)}: {string.Join(", ", hostedServicesWithoutStatus.Select(x => x.GetType().Name))}");
            }

            var notRunningServices = hostedServicesWithStatus
                .Where(x => x.Status == HostStatus.Stopping || x.Status == HostStatus.Stopped)
                .ToList();
            if (notRunningServices.Any())
            {
                return HealthCheckResult.Unhealthy($"Services are not running: {string.Join(", ", notRunningServices.Select(x => x.GetType().Name))}");
            }

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}
