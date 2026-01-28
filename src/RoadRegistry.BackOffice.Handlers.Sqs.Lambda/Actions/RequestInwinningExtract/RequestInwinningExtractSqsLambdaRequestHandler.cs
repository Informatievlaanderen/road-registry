namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.RequestInwinningExtract;

using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using RequestExtract;
using RoadRegistry.BackOffice.Abstractions.Extracts.V2;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Hosts;
using TicketingService.Abstractions;

public sealed class RequestInwinningExtractSqsLambdaRequestHandler : SqsLambdaHandler<RequestInwinningExtractSqsLambdaRequest>
{
    private readonly ExtractsDbContext _extractsDbContext;
    private readonly ExtractRequester _extractRequester;

    public RequestInwinningExtractSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        ExtractsDbContext extractsDbContext,
        ExtractRequester extractRequester,
        ILoggerFactory loggerFactory)
        : base(
            options,
            retryPolicy,
            ticketing,
            idempotentCommandHandler,
            roadRegistryContext,
            loggerFactory.CreateLogger<RequestInwinningExtractSqsLambdaRequestHandler>())
    {
        _extractsDbContext = extractsDbContext;
        _extractRequester = extractRequester;
    }

    protected override async Task<object> InnerHandle(RequestInwinningExtractSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        var niscode = request.Request.NisCode;
        var geometry = request.Request.Contour.Value;

        if (!request.Request.IsInformative)
        {
            var inwinningsZone = await _extractsDbContext.Inwinningszones.FindAsync([niscode], cancellationToken);

            if (inwinningsZone is not null && inwinningsZone.Operator != request.Provenance.Operator)
            {
                throw new ValidationException([
                    new ValidationFailure
                    {
                        PropertyName = string.Empty,
                        ErrorCode = "InwinningszoneGestart",
                        ErrorMessage = "Inwinning is al gestart door een andere organisatie."
                    }
                ]);
            }

            if (inwinningsZone is null)
            {
                inwinningsZone = new Inwinningszone
                {
                    NisCode = niscode,
                    Contour = geometry,
                    Operator = request.Provenance.Operator,
                    DownloadId = request.Request.DownloadId,
                    Completed = false
                };
                _extractsDbContext.Inwinningszones.Add(inwinningsZone);
            }
            else
            {
                inwinningsZone.DownloadId = request.Request.DownloadId;
            }
        }

        await _extractRequester.BuildExtract(
            new RequestExtractData(
                request.Request.ExtractRequestId,
                request.Request.DownloadId,
                geometry,
                request.Request.Description,
                request.Request.IsInformative,
                request.Request.IsInformative ? null : BuildExternalRequestId(niscode)
            ),
            new TicketId(request.TicketId),
            WellKnownZipArchiveWriterVersions.DomainV2_Inwinning,
            request.Provenance, cancellationToken);

        var downloadId = new DownloadId(request.Request.DownloadId);
        return new RequestExtractResponse(downloadId);
    }

    private static string BuildExternalRequestId(string niscode)
    {
        // don't change the format, it's used in backend and frontend
        return $"INWINNING_{niscode}";
    }

    protected override Task ValidateIfMatchHeaderValue(RequestInwinningExtractSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
