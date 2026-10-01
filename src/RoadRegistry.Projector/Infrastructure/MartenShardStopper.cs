namespace RoadRegistry.Projector.Infrastructure;

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JasperFx;
using JasperFx.Events.Daemon;
using Microsoft.Extensions.Logging;

// Stops a Marten projection shard and does not answer until it really is stopped.
//
// Marten's own stop is a drain, and an unbounded one. IProjectionDaemon.StopAgentAsync does bound it - it hands the
// agent a token with CancelAfter(5.Seconds()) - but that token never reaches the work: GroupedProjectionExecution
// .StopAndDrainAsync ignores the token it is given, waits on the block's WaitForCompletionAsync (which takes none),
// and only trips its own cancellation once the drain has finished. So the call returns when every page the daemon had
// already fetched has been projected, which on a projection that is behind is minutes to hours - and it holds the
// daemon's semaphore throughout, so every other start and stop queues up behind it.
//
// Hence: the drain gets a grace period, and then the batch in flight is cancelled by hard stopping the agent. That is
// not a lossy shortcut. A batch is committed as a whole, so a cancelled one was never committed at all and is replayed
// from the projection's last committed position when it is started again. What it throws away is the work that batch
// had done, not progress the projection had made.
public sealed class MartenShardStopper
{
    // After the drain returns, the daemon removes the agent from its registry and may stop the high water detection.
    // That is bookkeeping rather than stopping, so the caller is not made to wait long for it - only long enough that
    // a start right after this call does not race it.
    private static readonly TimeSpan BookkeepingTimeout = TimeSpan.FromSeconds(5);

    private readonly ILogger<MartenShardStopper> _logger;
    private readonly TimeSpan _hardStopTimeout;
    private readonly TimeSpan _pollingInterval;

    /// <param name="logger"></param>
    /// <param name="hardStopTimeout">How long the shard may take to stop after the batch it was working on has been
    /// cancelled. Short on purpose: once that batch is cancelled there is nothing left to wait for except the
    /// cancellation unwinding, so a shard still running after it is one that more waiting will not fix.</param>
    /// <param name="pollingInterval">How often the daemon is asked whether the shard has stopped.</param>
    public MartenShardStopper(ILogger<MartenShardStopper> logger, TimeSpan? hardStopTimeout = null, TimeSpan? pollingInterval = null)
    {
        _logger = logger;
        _hardStopTimeout = hardStopTimeout ?? TimeSpan.FromSeconds(30);
        _pollingInterval = pollingInterval ?? TimeSpan.FromMilliseconds(250);
    }

    /// <summary>
    /// Stops <paramref name="shardName"/>, returning only once the daemon reports it as no longer running.
    /// </summary>
    /// <param name="daemon">The running projection daemon.</param>
    /// <param name="shardName">The full shard identity, e.g. "RoadNetworkChangesWmsWfsV2Projection:All".</param>
    /// <param name="drainGracePeriod">How long the shard may take to finish the work it already has in hand before the
    /// batch it is working on is cancelled.</param>
    /// <param name="cancellationToken">Gives up waiting when the caller goes away. The stop itself carries on: the
    /// desired state is recorded before this is ever called, so an abandoned request still leaves the shard stopping.</param>
    public async Task<MartenShardStopOutcome> StopAsync(
        IProjectionDaemon daemon,
        string shardName,
        TimeSpan drainGracePeriod,
        CancellationToken cancellationToken)
    {
        // Deliberately not awaited: this is the call that can take forever. What it is doing - draining - is watched
        // through the daemon's own status below instead, and the task is picked up again at the end for its bookkeeping.
        var drain = daemon.StopAgentAsync(shardName);

        // Every path out of here can leave that task running behind us - the caller gives up, or the shard never stops -
        // so a failure in it is logged here rather than nowhere.
        _ = drain.ContinueWith(
            task => _logger.LogWarning(task.Exception, "The daemon reported an error while stopping {ShardName}.", shardName),
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted,
            TaskScheduler.Default);

        var outcome = MartenShardStopOutcome.Drained;

        if (!await WaitUntilStoppedAsync(daemon, shardName, drainGracePeriod, cancellationToken))
        {
            _logger.LogWarning(
                "{ShardName} has not stopped within {GracePeriodInSeconds}s; cancelling the batch it is working on. That batch was never committed and replays when the projection is started again.",
                shardName, drainGracePeriod.TotalSeconds);

            await CancelBatchInFlightAsync(daemon, shardName);
            outcome = MartenShardStopOutcome.BatchCancelled;

            if (!await WaitUntilStoppedAsync(daemon, shardName, _hardStopTimeout, cancellationToken))
            {
                _logger.LogError("{ShardName} is still running after the batch it was working on was cancelled.", shardName);
                return MartenShardStopOutcome.StillRunning;
            }
        }

        await AwaitBookkeepingAsync(drain, shardName, cancellationToken);

        return outcome;
    }

    // The daemon's status is the same thing the rebuild endpoint reads to decide whether it may touch the read model,
    // so a shard this reports as stopped is a shard the next call will be let through for.
    private async Task<bool> WaitUntilStoppedAsync(IProjectionDaemon daemon, string shardName, TimeSpan timeout, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        while (daemon.StatusFor(shardName) == AgentStatus.Running)
        {
            if (stopwatch.Elapsed >= timeout)
            {
                return false;
            }

            await Task.Delay(_pollingInterval, cancellationToken);
        }

        return true;
    }

    // Hard stopping the agent cancels the execution's own cancellation source, which is the token the projection's
    // handlers are given - so it lands in the batch that is being built rather than waiting for it.
    private async Task CancelBatchInFlightAsync(IProjectionDaemon daemon, string shardName)
    {
        var agent = daemon.CurrentAgents().FirstOrDefault(x => x.Name.Identity == shardName);
        if (agent is null)
        {
            // The drain finished between the wait giving up and this line. Nothing left to cancel.
            return;
        }

        try
        {
            await agent.HardStopAsync();
        }
        catch (Exception ex)
        {
            // A hard stop that throws has still cancelled what it needed to cancel; whether the shard actually stopped
            // is decided by the wait that follows, not by this.
            _logger.LogWarning(ex, "Hard stopping {ShardName} threw.", shardName);
        }
    }

    private async Task AwaitBookkeepingAsync(Task drain, string shardName, CancellationToken cancellationToken)
    {
        try
        {
            await drain.WaitAsync(BookkeepingTimeout, cancellationToken);
        }
        catch (TimeoutException)
        {
            // The shard is stopped either way - that was established above. Only the daemon's own tidying is still
            // running, and it does not hold up the answer.
            _logger.LogInformation("{ShardName} is stopped; the daemon is still finishing its own bookkeeping.", shardName);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // A drain that failed is already logged where it was started, and the shard is stopped regardless.
        }
    }
}

public enum MartenShardStopOutcome
{
    // The shard finished the work it had in hand and stopped by itself.
    Drained,

    // The drain was cut short: the batch in flight was cancelled and replays on the next start.
    BatchCancelled,

    // It did not stop, not even after the batch it was working on was cancelled.
    StillRunning
}
