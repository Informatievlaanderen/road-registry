namespace RoadRegistry.Projector.Infrastructure;

using System;
using System.Collections.Generic;

// How often the supervisor is allowed to restart a projection that is not running.
//
// A projection that fell over for a passing reason - a deadlock, a database that blipped - is back after one restart,
// so the first few attempts come quickly. One that is broken (a bad event, a schema it cannot write) will not be fixed
// by trying again every five minutes for a week: that only fills the log, and the Slack channel, with the same failure
// hundreds of times a day. After the fast attempts are spent it drops to one attempt an hour, which still recovers on
// its own from an outage that ends overnight without anyone watching.
//
// The count is per shard and resets as soon as that shard is seen running again, so a projection that recovers is back
// to fast restarts the next time it needs one.
public sealed class ProjectionRestartPolicy
{
    public const int DefaultFastAttempts = 3;
    public static readonly TimeSpan DefaultSlowInterval = TimeSpan.FromHours(1);

    private readonly int _fastAttempts;
    private readonly TimeSpan _slowInterval;
    private readonly Dictionary<string, (int Attempts, DateTimeOffset LastAttempt)> _attemptsByShard = new(StringComparer.OrdinalIgnoreCase);

    public ProjectionRestartPolicy(int? fastAttempts = null, TimeSpan? slowInterval = null)
    {
        _fastAttempts = fastAttempts ?? DefaultFastAttempts;
        _slowInterval = slowInterval ?? DefaultSlowInterval;
    }

    // How many times in a row this shard has been restarted without being seen running since.
    public int AttemptsFor(string shardName)
    {
        return _attemptsByShard.TryGetValue(shardName, out var attempt) ? attempt.Attempts : 0;
    }

    public bool IsBackingOff(string shardName)
    {
        return AttemptsFor(shardName) >= _fastAttempts;
    }

    public bool ShouldAttemptRestart(string shardName, DateTimeOffset now)
    {
        if (!_attemptsByShard.TryGetValue(shardName, out var attempt))
        {
            return true;
        }

        return attempt.Attempts < _fastAttempts || now - attempt.LastAttempt >= _slowInterval;
    }

    public void RecordAttempt(string shardName, DateTimeOffset now)
    {
        var attempts = AttemptsFor(shardName);
        _attemptsByShard[shardName] = (attempts + 1, now);
    }

    // Seen running: whatever was wrong is over, so the next failure gets the fast attempts again.
    public void RecordRunning(string shardName)
    {
        _attemptsByShard.Remove(shardName);
    }
}
