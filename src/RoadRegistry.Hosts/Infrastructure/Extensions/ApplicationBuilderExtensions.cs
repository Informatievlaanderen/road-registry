namespace Microsoft.AspNetCore.Builder;

using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseHealthChecks(this IApplicationBuilder app)
    {
        return app.UseHealthChecks(new PathString("/health"), new HealthCheckOptions
        {
            AllowCachingResponses = false,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        });
    }
}
