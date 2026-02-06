namespace RoadRegistry.BackOffice.Handlers.Extracts;

using RoadRegistry.Extracts.Schema;
using RoadRegistry.Infrastructure;

public class ExtractRequests : IExtractRequests
{
    private readonly ExtractsDbContext _extractsDbContext;

    public ExtractRequests(ExtractsDbContext extractsDbContext)
    {
        _extractsDbContext = extractsDbContext;
    }

    public async Task UploadAcceptedAsync(UploadId uploadId, CancellationToken cancellationToken)
    {
        await _extractsDbContext.UploadAcceptedAsync(uploadId, cancellationToken);
    }

    public async Task AutomaticValidationFailedAsync(UploadId uploadId, CancellationToken cancellationToken)
    {
        await _extractsDbContext.AutomaticValidationFailedAsync(uploadId, cancellationToken);
    }

    public async Task ManualValidationFailedAsync(UploadId uploadId, CancellationToken cancellationToken)
    {
        await _extractsDbContext.ManualValidationFailedAsync(uploadId, cancellationToken);
    }
}
