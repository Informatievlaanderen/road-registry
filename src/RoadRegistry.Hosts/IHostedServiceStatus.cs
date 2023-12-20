namespace RoadRegistry.Hosts;

public interface IHostedServiceStatus
{
    HostStatus Status { get; }
}

public enum HostStatus
{
    Provisioning,
    Starting,
    Running,
    Stopping,
    Stopped
}
