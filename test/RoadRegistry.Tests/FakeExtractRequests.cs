namespace RoadRegistry.Tests;

using RoadRegistry.Infrastructure;

public class FakeExtractRequests: IExtractRequests
{
    public Task UploadAcceptedAsync(UploadId uploadId, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task AutomaticValidationFailedAsync(UploadId uploadId, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task ManualValidationFailedAsync(UploadId uploadId, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
