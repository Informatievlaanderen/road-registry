namespace RoadRegistry.Projector.Projections;

using System;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure;
using JasperFx;
using JasperFx.Events.Daemon;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadRegistry.Pbs.Projections;
using RoadRegistry.Pbs.Schema;

public partial class ProjectionsController
{
    private static int _pbsBackfillRunning;

    /// <summary>
    /// Derives every AfgeleideWegsegment of the PBS read model again, afgeleide schijnknopen included (GAWR-7836).
    /// </summary>
    /// <remarks>
    /// A one-time backfill for the segments projected before the afgeleide schijnknopen existed. It runs from the
    /// segment and attribute rows the read model already holds, no event is replayed.
    ///
    /// RoadNetworkChangesPbsProjection has to be stopped first (via {id}/stop); a running projection is refused with a
    /// 409, because the backfill writes the same rows. The call returns as soon as the backfill has started; it runs in
    /// the background, logs its progress per chunk, and stops by itself when the projection is started again before it
    /// is done. Start the projection once the log says the backfill completed. Running it again is harmless, and
    /// fromSegmentId resumes it after the last WS_OIDN it logged.
    /// </remarks>
    /// <param name="daemonAccessor"></param>
    /// <param name="lifetime"></param>
    /// <param name="chunkSize">How many road segments are derived and committed together.</param>
    /// <param name="fromSegmentId">The WS_OIDN to start from.</param>
    [HttpPost("roadnetworkchangespbsprojection/backfill-afgeleide-schijnknopen")]
    public IActionResult BackfillPbsDerivedSchijnknopen(
        [FromServices] MartenProjectionDaemonAccessor daemonAccessor,
        [FromServices] IHostApplicationLifetime lifetime,
        [FromQuery] int chunkSize = 1000,
        [FromQuery] int fromSegmentId = 0)
    {
        var projection = FindMartenProjection(WellKnownProjectionStateNames.RoadNetworkChangesPbsProjection);
        if (projection is null)
        {
            return NotFound($"Projection {WellKnownProjectionStateNames.RoadNetworkChangesPbsProjection} is not known or not enabled on this host.");
        }

        var daemon = daemonAccessor.Daemon;
        if (daemon is null)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, "The Marten projection daemon is not running.");
        }

        var shardName = projection.Id;
        if (daemon.StatusFor(shardName) == AgentStatus.Running)
        {
            return Conflict($"{shardName} is still running; stop it via projections/{shardName[..shardName.IndexOf(':')]}/stop before backfilling.");
        }

        if (Interlocked.CompareExchange(ref _pbsBackfillRunning, 1, 0) != 0)
        {
            return Conflict("The backfill of the afgeleide schijnknopen is already running.");
        }

        var dbContextFactory = HttpContext.RequestServices.GetRequiredService<IDbContextFactory<PbsContext>>();
        var logger = _logger;
        var stopping = lifetime.ApplicationStopping;

        _ = Task.Run(async () =>
        {
            try
            {
                await DerivedSchijnknopenBackfill.RunAsync(dbContextFactory, logger,
                    () => daemonAccessor.Daemon?.StatusFor(shardName) != AgentStatus.Running,
                    Math.Clamp(chunkSize, 1, 10_000), fromSegmentId, stopping);
            }
            catch (OperationCanceledException) when (stopping.IsCancellationRequested)
            {
                logger.LogWarning("Backfill of the afgeleide schijnknopen was cancelled because the host is stopping; see the last WS_OIDN it logged to resume.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Backfill of the afgeleide schijnknopen failed; see the last WS_OIDN it logged to resume.");
            }
            finally
            {
                Interlocked.Exchange(ref _pbsBackfillRunning, 0);
            }
        }, CancellationToken.None);

        return Accepted($"The backfill of the afgeleide schijnknopen started from WS_OIDN {fromSegmentId}; follow its progress in the log.");
    }
}
