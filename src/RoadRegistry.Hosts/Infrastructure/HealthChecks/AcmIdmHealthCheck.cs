namespace RoadRegistry.Hosts.Infrastructure.HealthChecks;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Options;

public class AcmIdmHealthCheck : IHealthCheck
{
    private readonly AcmIdmHealthCheckOptions _options;

    public AcmIdmHealthCheck(AcmIdmHealthCheckOptions options)
    {
        _options = options;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new())
    {
        throw new NotImplementedException();
    }
}
