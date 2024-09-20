namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers;

using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Swashbuckle.AspNetCore.Annotations;
using SystemHealthCheck;

public partial class RoadRegistrySystemController
{
    private const string HealthCheckRoute = "healthcheck";

    /// <summary>
    ///     Checks if all core components are still operational.
    /// </summary>
    /// <param name="healthCheckService"></param>
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    /// <returns>IActionResult.</returns>
    [HttpPost(HealthCheckRoute, Name = nameof(HealthCheck))]
    [SwaggerOperation(OperationId = nameof(HealthCheck), Description = "")]
    public async Task<IActionResult> HealthCheck(
        [FromServices] ISystemHealthCheckService healthCheckService,
        CancellationToken cancellationToken)
    {
        var report = await healthCheckService.CheckHealthAsync(cancellationToken);

        if (report.Status == HealthStatus.Healthy)
        {
            return Ok(report);
        }

        return new ObjectResult(report)
        {
            StatusCode = (int)HttpStatusCode.ServiceUnavailable
        };
    }
}
