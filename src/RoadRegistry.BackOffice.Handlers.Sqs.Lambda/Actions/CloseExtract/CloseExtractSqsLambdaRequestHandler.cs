namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.CloseExtract;

using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.Abstractions.Extracts.V2;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Hosts;
using TicketingService.Abstractions;

public sealed class CloseExtractSqsLambdaRequestHandler : SqsLambdaHandler<CloseExtractSqsLambdaRequest>
{
    private readonly ExtractsDbContext _extractsDbContext;

    public CloseExtractSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        ExtractsDbContext extractsDbContext,
        ILogger<CloseExtractSqsLambdaRequestHandler> logger)
        : base(
            options,
            retryPolicy,
            ticketing,
            idempotentCommandHandler,
            roadRegistryContext,
            logger)
    {
        _extractsDbContext = extractsDbContext;
    }

    protected override async Task<object> InnerHandle(CloseExtractSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        var download = await _extractsDbContext.ExtractDownloads.FindAsync([request.Request.DownloadId], cancellationToken);
        if (download is null)
        {
            throw new ExtractRequestNotFoundException(new DownloadId(request.Request.DownloadId));
        }

        download.Closed = true;

        await _extractsDbContext.SaveChangesAsync(cancellationToken);

        return new CloseExtractResponse();
    }

    protected override Task ValidateIfMatchHeaderValue(CloseExtractSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
