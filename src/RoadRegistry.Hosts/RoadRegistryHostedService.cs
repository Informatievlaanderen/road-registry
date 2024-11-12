namespace RoadRegistry.Hosts;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public abstract class RoadRegistryHostedService: IHostedService, IHostedServiceStatus
{
    protected ILogger Logger { get; }

    protected RoadRegistryHostedService(ILoggerFactory loggerFactory)
    {
        Logger = loggerFactory.CreateLogger(GetType()).ThrowIfNull();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Status = HostStatus.Starting;
        Logger.LogInformation("{Name} starting...", GetType().Name);
        await StartingAsync(cancellationToken);
        Status = HostStatus.Running;
        Logger.LogInformation("{Name} started.", GetType().Name);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        Status = HostStatus.Stopping;
        Logger.LogInformation("{Name} stopping...", GetType().Name);
        await StoppingAsync(cancellationToken);
        Status = HostStatus.Stopped;
        Logger.LogInformation("{Name} stopped.", GetType().Name);
    }

    public HostStatus Status { get; private set; } = HostStatus.Provisioning;

    protected abstract Task StartingAsync(CancellationToken cancellationToken);
    protected abstract Task StoppingAsync(CancellationToken cancellationToken);
}
