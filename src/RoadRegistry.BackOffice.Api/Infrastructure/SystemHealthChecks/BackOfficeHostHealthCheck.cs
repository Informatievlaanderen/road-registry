namespace RoadRegistry.BackOffice.Api.Infrastructure.SystemHealthChecks
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Diagnostics.HealthChecks;

    internal class BackOfficeHostHealthCheck : ISystemHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
        {
            //TODO-rik Start een dummy command voor de commandhost/eventhost/extracthost

            return HealthCheckResult.Healthy();
        }
    }
}
