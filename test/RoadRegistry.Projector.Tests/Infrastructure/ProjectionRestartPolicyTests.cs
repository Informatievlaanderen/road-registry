namespace RoadRegistry.Projector.Tests.Infrastructure;

using System;
using RoadRegistry.Projector.Infrastructure;

public class ProjectionRestartPolicyTests
{
    private const string ShardName = "RoadNetworkChangesReadProjection:All";

    private static readonly DateTimeOffset Now = new(2026, 9, 3, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void A_shard_that_has_never_failed_is_restarted_immediately()
    {
        var policy = new ProjectionRestartPolicy();

        Assert.True(policy.ShouldAttemptRestart(ShardName, Now));
    }

    [Fact]
    public void The_first_attempts_happen_on_every_tick()
    {
        var policy = new ProjectionRestartPolicy(fastAttempts: 3, slowInterval: TimeSpan.FromHours(1));

        // Ticks are five minutes apart, well inside the slow interval: what lets these through is the attempt count.
        for (var attempt = 0; attempt < 3; attempt++)
        {
            var tick = Now.AddMinutes(5 * attempt);

            Assert.True(policy.ShouldAttemptRestart(ShardName, tick));
            policy.RecordAttempt(ShardName, tick);
        }

        Assert.Equal(3, policy.AttemptsFor(ShardName));
    }

    [Fact]
    public void After_the_fast_attempts_are_spent_it_waits_for_the_slow_interval()
    {
        var policy = ExhaustFastAttempts();
        var lastAttempt = Now.AddMinutes(10);

        Assert.True(policy.IsBackingOff(ShardName));
        Assert.False(policy.ShouldAttemptRestart(ShardName, lastAttempt.AddMinutes(5)));
        Assert.False(policy.ShouldAttemptRestart(ShardName, lastAttempt.AddMinutes(59)));
        Assert.True(policy.ShouldAttemptRestart(ShardName, lastAttempt.AddHours(1)));
    }

    [Fact]
    public void A_slow_attempt_starts_the_hour_over_again()
    {
        var policy = ExhaustFastAttempts();
        var slowAttempt = Now.AddHours(2);

        policy.RecordAttempt(ShardName, slowAttempt);

        Assert.False(policy.ShouldAttemptRestart(ShardName, slowAttempt.AddMinutes(30)));
        Assert.True(policy.ShouldAttemptRestart(ShardName, slowAttempt.AddHours(1)));
    }

    [Fact]
    public void A_shard_that_is_seen_running_gets_its_fast_attempts_back()
    {
        var policy = ExhaustFastAttempts();

        policy.RecordRunning(ShardName);

        Assert.Equal(0, policy.AttemptsFor(ShardName));
        Assert.False(policy.IsBackingOff(ShardName));
        Assert.True(policy.ShouldAttemptRestart(ShardName, Now.AddMinutes(15)));
    }

    [Fact]
    public void Shards_are_counted_separately()
    {
        var policy = ExhaustFastAttempts();
        const string otherShard = "RoadNetworkChangesPbsProjection:All";

        Assert.False(policy.ShouldAttemptRestart(ShardName, Now.AddMinutes(15)));
        Assert.True(policy.ShouldAttemptRestart(otherShard, Now.AddMinutes(15)));
    }

    private static ProjectionRestartPolicy ExhaustFastAttempts()
    {
        var policy = new ProjectionRestartPolicy(fastAttempts: 3, slowInterval: TimeSpan.FromHours(1));

        for (var attempt = 0; attempt < 3; attempt++)
        {
            policy.RecordAttempt(ShardName, Now.AddMinutes(5 * attempt));
        }

        return policy;
    }
}
