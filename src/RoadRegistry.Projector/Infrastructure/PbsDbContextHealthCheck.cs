namespace RoadRegistry.Projector.Infrastructure;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RoadRegistry.Pbs.Schema;

// Health check for the PBS read model. The PbsContext is registered as an IDbContextFactory (not a scoped DbContext),
// so we cannot use AddDbContextCheck<PbsContext>; this verifies connectivity through the factory instead.
public sealed class PbsDbContextHealthCheck : IHealthCheck
{
    private readonly IDbContextFactory<PbsContext> _dbContextFactory;

    public PbsDbContextHealthCheck(IDbContextFactory<PbsContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
            return canConnect
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("Cannot connect to the PBS database.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("PBS database health check failed.", ex);
        }
    }
}
