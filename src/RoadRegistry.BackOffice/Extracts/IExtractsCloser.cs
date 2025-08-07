namespace RoadRegistry.BackOffice.Extracts;

using System.Threading;
using System.Threading.Tasks;

public interface IExtractsCloser
{
    Task CloseAsync(DownloadId downloadId, CancellationToken cancellationToken);
}
