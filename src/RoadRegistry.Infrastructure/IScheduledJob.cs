namespace RoadRegistry.Infrastructure;

using System.Threading;
using System.Threading.Tasks;

public interface IScheduledJob
{
    Task RunAsync(CancellationToken cancellationToken);
}
