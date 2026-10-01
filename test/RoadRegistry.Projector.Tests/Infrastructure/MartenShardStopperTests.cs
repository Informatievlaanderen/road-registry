namespace RoadRegistry.Projector.Tests.Infrastructure;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JasperFx;
using JasperFx.Events.Daemon;
using JasperFx.Events.Projections;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RoadRegistry.Projector.Infrastructure;

public class MartenShardStopperTests
{
    private const string ShardName = "RoadNetworkChangesWmsWfsV2Projection:All";

    // Short enough to keep the tests quick, long enough that they are not decided by the scheduler.
    private static readonly TimeSpan Grace = TimeSpan.FromMilliseconds(200);
    private static readonly TimeSpan PollingInterval = TimeSpan.FromMilliseconds(10);

    [Fact]
    public async Task A_shard_that_stops_on_its_own_keeps_the_batch_it_was_working_on()
    {
        var daemon = new FakeDaemon();
        daemon.StopsWhenAsked = true;

        var outcome = await Stopper().StopAsync(daemon, ShardName, Grace, CancellationToken.None);

        Assert.Equal(MartenShardStopOutcome.Drained, outcome);
        Assert.False(daemon.Agent.WasHardStopped);
    }

    [Fact]
    public async Task A_shard_that_does_not_stop_within_the_grace_period_has_its_batch_cancelled()
    {
        // The drain that never returns: Marten's StopAgentAsync waiting for a page that takes minutes to project.
        var daemon = new FakeDaemon();
        daemon.StopsWhenAsked = false;

        var outcome = await Stopper().StopAsync(daemon, ShardName, Grace, CancellationToken.None);

        Assert.Equal(MartenShardStopOutcome.BatchCancelled, outcome);
        Assert.True(daemon.Agent.WasHardStopped);
    }

    [Fact]
    public async Task A_shard_that_survives_having_its_batch_cancelled_is_reported_as_still_running()
    {
        var daemon = new FakeDaemon();
        daemon.StopsWhenAsked = false;
        daemon.Agent.StopsWhenHardStopped = false;

        var outcome = await Stopper().StopAsync(daemon, ShardName, Grace, CancellationToken.None);

        Assert.Equal(MartenShardStopOutcome.StillRunning, outcome);
        Assert.True(daemon.Agent.WasHardStopped);
    }

    [Fact]
    public async Task A_shard_that_has_already_stopped_is_left_alone()
    {
        var daemon = new FakeDaemon();
        daemon.Status = AgentStatus.Stopped;
        daemon.StopsWhenAsked = true;

        var outcome = await Stopper().StopAsync(daemon, ShardName, Grace, CancellationToken.None);

        Assert.Equal(MartenShardStopOutcome.Drained, outcome);
        Assert.False(daemon.Agent.WasHardStopped);
    }

    [Fact]
    public async Task Giving_up_on_the_wait_does_not_take_the_stop_back()
    {
        var daemon = new FakeDaemon();
        daemon.StopsWhenAsked = false;
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => Stopper().StopAsync(daemon, ShardName, TimeSpan.FromMinutes(1), cancellation.Token));

        // The caller walking away is not a reason for the shard to stay up: the daemon was told to stop it before the
        // wait ever started, and that stands.
        Assert.True(daemon.WasAskedToStop);
    }

    private static MartenShardStopper Stopper()
    {
        return new MartenShardStopper(
            NullLogger<MartenShardStopper>.Instance,
            hardStopTimeout: Grace,
            pollingInterval: PollingInterval);
    }

    // A daemon with one agent for ShardName. Only the four members the stopper uses do anything; the rest of
    // IProjectionDaemon is left to Moq, which would fail loudly if the stopper started calling it.
    private sealed class FakeDaemon : IProjectionDaemon
    {
        private readonly IProjectionDaemon _rest = new Mock<IProjectionDaemon>(MockBehavior.Strict).Object;

        // Stands in for the drain: the task Marten's StopAgentAsync hands back, which only completes once the shard
        // has actually come to a halt.
        private readonly TaskCompletionSource _drain = new();

        public FakeDaemon()
        {
            Agent = new FakeAgent(Stop);
        }

        public FakeAgent Agent { get; }
        public AgentStatus Status { get; set; } = AgentStatus.Running;

        // Whether the graceful stop returns at all - the whole point of the stopper is that it usually does not.
        public bool StopsWhenAsked { get; set; }
        public bool WasAskedToStop { get; private set; }

        public AgentStatus StatusFor(string shardName)
        {
            return shardName == ShardName ? Status : AgentStatus.Stopped;
        }

        public Task StopAgentAsync(string shardName, Exception ex = null)
        {
            WasAskedToStop = true;

            if (StopsWhenAsked)
            {
                Stop();
            }

            return _drain.Task;
        }

        private void Stop()
        {
            Status = AgentStatus.Stopped;
            _drain.TrySetResult();
        }

        public IReadOnlyList<ISubscriptionAgent> CurrentAgents()
        {
            return Status == AgentStatus.Running ? [Agent] : [];
        }

        public void Dispose()
        {
        }

        public ShardStateTracker Tracker => _rest.Tracker;
        public bool IsRunning => _rest.IsRunning;
        public Task PrepareForRebuildsAsync() => _rest.PrepareForRebuildsAsync();
        public Task RebuildProjectionAsync(string projectionName, CancellationToken token) => _rest.RebuildProjectionAsync(projectionName, token);
        public Task RebuildProjectionAsync<TView>(CancellationToken token) => _rest.RebuildProjectionAsync<TView>(token);
        public Task RebuildProjectionAsync(Type projectionType, CancellationToken token) => _rest.RebuildProjectionAsync(projectionType, token);
        public Task RebuildProjectionAsync(Type projectionType, TimeSpan shardTimeout, CancellationToken token) => _rest.RebuildProjectionAsync(projectionType, shardTimeout, token);
        public Task RebuildProjectionAsync(string projectionName, TimeSpan shardTimeout, CancellationToken token) => _rest.RebuildProjectionAsync(projectionName, shardTimeout, token);
        public Task RebuildProjectionAsync<TView>(TimeSpan shardTimeout, CancellationToken token) => _rest.RebuildProjectionAsync<TView>(shardTimeout, token);
        public Task StartAgentAsync(string shardName, CancellationToken token) => _rest.StartAgentAsync(shardName, token);
        public Task<ISubscriptionAgent> StartAgentAsync(ShardName name, CancellationToken token) => _rest.StartAgentAsync(name, token);
        public Task StopAgentAsync(ShardName shardName, Exception ex = null) => StopAgentAsync(shardName.Identity, ex);
        public Task StartAllAsync() => _rest.StartAllAsync();
        public Task StopAllAsync() => _rest.StopAllAsync();
        public Task WaitForNonStaleData(TimeSpan timeout) => _rest.WaitForNonStaleData(timeout);
        public long HighWaterMark() => _rest.HighWaterMark();
        public bool HasAnyPaused() => _rest.HasAnyPaused();
        public void EjectPausedShard(string shardName) => _rest.EjectPausedShard(shardName);
        public Task WaitForShardToBeRunning(string shardName, TimeSpan timeout) => _rest.WaitForShardToBeRunning(shardName, timeout);
        public Task RewindSubscriptionAsync(string subscriptionName, CancellationToken token, long? sequenceFloor = 0, DateTimeOffset? timestamp = null) => _rest.RewindSubscriptionAsync(subscriptionName, token, sequenceFloor, timestamp);
    }

    private sealed class FakeAgent : ISubscriptionAgent
    {
        private readonly ISubscriptionAgent _rest = new Mock<ISubscriptionAgent>(MockBehavior.Strict).Object;
        private readonly Action _stop;

        public FakeAgent(Action stop)
        {
            _stop = stop;
        }

        public bool WasHardStopped { get; private set; }

        // A hard stop cancels the batch being built, which is what normally ends the projection's run. A shard that
        // keeps running through it is the case where there is nothing left to try.
        public bool StopsWhenHardStopped { get; set; } = true;

        public ShardName Name { get; } = new("RoadNetworkChangesWmsWfsV2Projection");

        public Task HardStopAsync()
        {
            WasHardStopped = true;

            if (StopsWhenHardStopped)
            {
                _stop();
            }

            return Task.CompletedTask;
        }

        public long Position => _rest.Position;
        public AgentStatus Status => _rest.Status;
        public DateTimeOffset? PausedTime => _rest.PausedTime;
        public ISubscriptionMetrics Metrics => _rest.Metrics;
        public ShardExecutionMode Mode => _rest.Mode;
        public ErrorHandlingOptions ErrorOptions => _rest.ErrorOptions;
        public AsyncOptions Options => _rest.Options;
        public void MarkHighWater(long sequence) => _rest.MarkHighWater(sequence);
        public void MarkSkipped(long sequence) => _rest.MarkSkipped(sequence);
        public Task StopAndDrainAsync(CancellationToken token) => _rest.StopAndDrainAsync(token);
        public Task StartAsync(SubscriptionExecutionRequest request) => _rest.StartAsync(request);
        public Task RecordDeadLetterEventAsync(DeadLetterEvent @event) => _rest.RecordDeadLetterEventAsync(@event);
        public Task ReplayAsync(SubscriptionExecutionRequest request, long highWaterMark, TimeSpan timeout) => _rest.ReplayAsync(request, highWaterMark, timeout);
        public ValueTask MarkSuccessAsync(long processedCeiling) => _rest.MarkSuccessAsync(processedCeiling);
        public Task ReportCriticalFailureAsync(Exception ex) => _rest.ReportCriticalFailureAsync(ex);
        public Task ReportCriticalFailureAsync(Exception ex, long lastProcessed) => _rest.ReportCriticalFailureAsync(ex, lastProcessed);
        public Task RecordDeadLetterEventAsync(JasperFx.Events.IEvent @event, Exception ex) => _rest.RecordDeadLetterEventAsync(@event, ex);
    }
}
