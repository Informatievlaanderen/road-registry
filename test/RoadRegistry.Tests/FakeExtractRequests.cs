namespace RoadRegistry.Tests;

using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.Infrastructure;

public class FakeExtractRequests: IExtractRequests
{
    public Task UploadAcceptedAsync(DownloadId downloadId, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task UploadAutomaticValidationFailedAsync(DownloadId downloadId, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
