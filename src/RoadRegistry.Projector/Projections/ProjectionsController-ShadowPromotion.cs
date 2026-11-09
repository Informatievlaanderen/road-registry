namespace RoadRegistry.Projector.Projections;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure;
using JasperFx;
using JasperFx.Events.Daemon;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice;
using RoadRegistry.Infrastructure.MartenDb.Projections;

public partial class ProjectionsController
{
    // The live shard of the read model being promoted, so its projection is not started, rebuilt or backfilled under
    // the swap. One promotion at a time.
    private static string? _promotingShard;

    /// <summary>
    /// Makes the shadow read model the live one.
    /// </summary>
    /// <remarks>
    /// The shadow projection has to be switched off again first (Projections:{Pbs|WmsWfsV2}:Shadow:Enabled = false,
    /// deployed), and its tables have to be there with a position and a Marten progression; otherwise the call is
    /// refused with a 400. Whether the shadow got far enough is up to the caller: whatever it is short of, the live
    /// projection catches up on after the swap.
    ///
    /// The live projection is stopped for the duration of the promotion and started again afterwards, unless it was
    /// meant to be stopped. The call answers once the promotion is done. A promotion that was interrupted after its
    /// SQL Server half is finished by calling this again, or by the next start of the host.
    /// </remarks>
    /// <param name="id">The live or the shadow projection id, case-insensitive, with or without the ":All" shard suffix
    /// (e.g. "RoadNetworkChangesPbsProjection" or "RoadNetworkChangesPbsTempProjection").</param>
    /// <param name="promoters"></param>
    /// <param name="daemonAccessor"></param>
    /// <param name="shardStopper"></param>
    /// <param name="lifetime"></param>
    /// <param name="drainSeconds">How long the live projection may take to finish the work it already has in hand
    /// before the batch it is working on is cancelled.</param>
    [HttpPost("{id}/promote")]
    public async Task<IActionResult> PromoteShadowReadModel(
        [FromRoute] string id,
        [FromServices] IEnumerable<ShadowReadModelPromoter> promoters,
        [FromServices] MartenProjectionDaemonAccessor daemonAccessor,
        [FromServices] MartenShardStopper shardStopper,
        [FromServices] IHostApplicationLifetime lifetime,
        [FromQuery] int drainSeconds = 30)
    {
        var promoter = FindShadowReadModelPromoter(promoters, id);
        if (promoter is null)
        {
            return NotFound($"Projection {id} has no shadow read model, or it is not enabled on this host.");
        }

        var daemon = daemonAccessor.Daemon;
        if (daemon is null)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, "The Marten projection daemon is not running.");
        }

        var readModel = promoter.ReadModel;
        var liveShard = readModel.LiveShardName;

        if (liveShard == WellKnownProjectionStateNames.RoadNetworkChangesPbsProjection && Volatile.Read(ref _pbsBackfillRunning) != 0)
        {
            return Conflict($"The {readModel.Name} shadow read model cannot be promoted while the backfill of the afgeleide schijnknopen is running; wait until the log says it completed.");
        }

        if (Interlocked.CompareExchange(ref _promotingShard, liveShard, null) is not null)
        {
            return Conflict("A shadow read model is already being promoted; wait until that call has answered.");
        }

        try
        {
            // Not the request's token: a promotion is not something to abandon halfway because a load balancer gave up
            // waiting for the answer. Only the host stopping ends it, and the next start finishes what it can.
            var cancellationToken = lifetime.ApplicationStopping;

            var pending = await promoter.IsPromotionPendingAsync(cancellationToken);
            if (!pending)
            {
                var decision = await promoter.DecideAsync(cancellationToken);
                if (!decision.Promote)
                {
                    return BadRequest($"The {readModel.Name} shadow read model in [{readModel.ShadowSchema}] cannot be promoted: {decision.Reason}");
                }
            }

            // Stopped for the duration: it would write into tables that are about to be dropped, and write back the
            // progression the promotion replaces. Its desired state says stopped, so the supervisor leaves it alone.
            var liveProjection = FindMartenProjection(liveShard);
            var desiredState = liveProjection is null
                ? ProjectionDesiredStates.Subscribed
                : (await _projectionStateStore.GetDesiredStatesAsync(cancellationToken)).DesiredStateOf(liveProjection);

            await _projectionStateStore.SetDesiredStateAsync(liveShard, ProjectionDesiredStates.Stopped, cancellationToken);

            if (daemon.StatusFor(liveShard) == AgentStatus.Running)
            {
                var drainGracePeriod = TimeSpan.FromSeconds(Math.Clamp(drainSeconds, 0, (int)TimeSpan.FromHours(1).TotalSeconds));
                var outcome = await shardStopper.StopAsync(daemon, liveShard, drainGracePeriod, cancellationToken);
                if (outcome == MartenShardStopOutcome.StillRunning)
                {
                    await _projectionStateStore.SetDesiredStateAsync(liveShard, desiredState, cancellationToken);
                    return StatusCode(StatusCodes.Status504GatewayTimeout,
                        $"{liveShard} did not stop, so the {readModel.Name} shadow read model was not promoted; check the logs.");
                }
            }

            string message;
            try
            {
                if (pending)
                {
                    await promoter.FinishPendingPromotionAsync(cancellationToken);
                    message = $"The {readModel.Name} shadow read model was already swapped into [{readModel.LiveSchema}]; its Marten progressions were moved now, so it is the live one.";
                }
                else
                {
                    var result = await promoter.PromoteAsync(cancellationToken);
                    message = $"The {readModel.Name} shadow read model is now the live one: {result.Tables} table(s) moved from [{readModel.ShadowSchema}] to [{readModel.LiveSchema}], replacing {result.ReplacedTables}; put back {result.Views} view(s), {result.ViewIndexes} view index(es) and {result.Permissions} permission(s).";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Promoting the {ReadModel} shadow read model failed.", readModel.Name);

                // Swapped but not finished: the live projection would continue from a progression that belongs to the
                // tables that were dropped. It stays stopped until the promotion is finished.
                if (await promoter.IsPromotionPendingAsync(CancellationToken.None))
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        $"The {readModel.Name} shadow read model was swapped into [{readModel.LiveSchema}], but moving its Marten progressions failed: {ex.Message} {liveShard} stays stopped; call this again to finish the promotion.");
                }

                // Nothing was swapped: the live read model is as it was, so its projection goes back to what it was doing.
                await RestoreLiveProjectionAsync(daemon, liveShard, desiredState, CancellationToken.None);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Promoting the {readModel.Name} shadow read model failed before anything was swapped: {ex.Message} The live read model is unchanged.");
            }

            await RestoreLiveProjectionAsync(daemon, liveShard, desiredState, cancellationToken);
            return Ok(desiredState.ShouldBeRunning()
                ? $"{message} {liveShard} was started again."
                : $"{message} {liveShard} stays stopped, as it was before.");
        }
        finally
        {
            Interlocked.Exchange(ref _promotingShard, null);
        }
    }

    private async Task RestoreLiveProjectionAsync(IProjectionDaemon daemon, string liveShard, string desiredState, CancellationToken cancellationToken)
    {
        await _projectionStateStore.SetDesiredStateAsync(liveShard, desiredState, cancellationToken);

        if (desiredState.ShouldBeRunning() && daemon.StatusFor(liveShard) != AgentStatus.Running)
        {
            await daemon.StartAgentAsync(liveShard, cancellationToken);
        }
    }

    private static bool IsBeingPromoted(string shardName)
    {
        return Volatile.Read(ref _promotingShard) == shardName;
    }

    private static ShadowReadModelPromoter? FindShadowReadModelPromoter(IEnumerable<ShadowReadModelPromoter> promoters, string id)
    {
        return promoters.FirstOrDefault(promoter =>
        {
            var readModel = promoter.ReadModel;
            return new[] { readModel.LiveProjectionName, readModel.LiveShardName, readModel.ShadowProjectionName, readModel.ShadowShardName }
                .Any(name => string.Equals(name, id, StringComparison.OrdinalIgnoreCase));
        });
    }
}
