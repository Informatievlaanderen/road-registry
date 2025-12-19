namespace RoadRegistry.Hosts;

using System;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Extensions;
using Microsoft.Extensions.Hosting;

public abstract class RoadRegistryBackgroundService : BackgroundService, IHostedServiceStatus
{
    protected ILogger Logger { get; }

    protected RoadRegistryBackgroundService(ILogger logger)
    {
        Logger = logger.ThrowIfNull();
    }

    public HostStatus Status { get; private set; } = HostStatus.Running;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Status = HostStatus.Running;
        Logger.LogInformation("{Name} started.", GetType().Name);
        await ExecutingAsync(stoppingToken);
    }

    protected abstract Task ExecutingAsync(CancellationToken cancellationToken);
}
