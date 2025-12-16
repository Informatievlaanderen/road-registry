namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers.Extracts;

using Abstractions.Extracts.V2;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using FluentValidation;
using FluentValidation.Results;
using Hosts;
using Infrastructure;
using Microsoft.Extensions.Logging;
using Requests.Extracts;
using RoadRegistry.Extracts.Schema;
using TicketingService.Abstractions;

public sealed class RequestInwinningExtractSqsLambdaRequestHandler : SqsLambdaHandler<RequestInwinningExtractSqsLambdaRequest>
{
    private readonly ExtractsDbContext _extractsDbContext;
    private readonly ExtractsEngine _extractsEngine;

    public RequestInwinningExtractSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        ExtractsDbContext extractsDbContext,
        ExtractsEngine extractsEngine,
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
        _extractsEngine = extractsEngine;
    }

    protected override async Task<object> InnerHandle(RequestInwinningExtractSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        var niscode = request.Request.NisCode;
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
                Contour = request.Request.Contour,
                Operator = request.Provenance.Operator,
                Completed = false
            };
            _extractsDbContext.Inwinningszones.Add(inwinningsZone);
        }

        await _extractsEngine.BuildExtract(
            new RequestExtractRequest(
                request.Request.ExtractRequestId,
                request.Request.DownloadId,
                request.Request.Contour,
                $"Data-inwinning {niscode}",
                false,
                $"INWINNING_{niscode}"),
            new TicketId(request.TicketId), request.Provenance, cancellationToken);

        var downloadId = new DownloadId(request.Request.DownloadId);
        return new RequestExtractResponse(downloadId);
    }

    protected override Task ValidateIfMatchHeaderValue(RequestInwinningExtractSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
