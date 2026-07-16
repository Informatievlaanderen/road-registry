namespace RoadRegistry.Tests;

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

public static class BackgroundServiceExtensions
{
    public static async Task RunOnceAsync(this BackgroundService service, CancellationToken cancellationToken = default)
    {
        await service.StartAsync(cancellationToken);
        await (service.ExecuteTask ?? Task.CompletedTask);
    }
}
