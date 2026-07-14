namespace RoadRegistry.Projector.Infrastructure;

using JasperFx.Events.Daemon;

// Shares the running Marten projection daemon so background services (e.g. the PBS projection supervisor) can act on
// individual shards without building a second daemon. Set by MartenProjectionsDaemonHostedService once the daemon has
// started, cleared when it stops.
public sealed class MartenProjectionDaemonAccessor
{
    private volatile IProjectionDaemon _daemon;

    public IProjectionDaemon Daemon
    {
        get => _daemon;
        set => _daemon = value;
    }
}
