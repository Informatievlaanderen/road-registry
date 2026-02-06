namespace RoadRegistry.Infrastructure;

using System.Threading;
using System.Threading.Tasks;

public interface IExtractRequests
{
    Task UploadAcceptedAsync(UploadId uploadId, CancellationToken cancellationToken);
    Task AutomaticValidationFailedAsync(UploadId uploadId, CancellationToken cancellationToken);
    Task ManualValidationFailedAsync(UploadId uploadId, CancellationToken cancellationToken);
}
