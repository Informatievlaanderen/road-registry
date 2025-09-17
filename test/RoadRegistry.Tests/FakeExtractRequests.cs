﻿namespace RoadRegistry.Tests;

using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Extracts;

public class FakeExtractRequests: IExtractRequests
{
    public Task UploadAcceptedAsync(DownloadId downloadId, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task UploadRejectedAsync(DownloadId downloadId, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
