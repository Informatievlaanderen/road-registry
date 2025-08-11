namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers.Extracts;

using Abstractions.Extracts.V2;
using BackOffice.Extracts;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Hosts;
using Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Requests.Extracts;
using RoadRegistry.Extracts.Schema;
using TicketingService.Abstractions;

public sealed class RequestExtractSqsLambdaRequestHandler : SqsLambdaHandler<RequestExtractSqsLambdaRequest>
{
    private readonly WKTReader _wktReader;
    private readonly ExtractsDbContext _extractsDbContext;
    private readonly RoadNetworkExtractDownloadsBlobClient _downloadsBlobClient;
    private readonly IRoadNetworkExtractArchiveAssembler _assembler;

    public RequestExtractSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        ExtractsDbContext extractsDbContext,
        RoadNetworkExtractDownloadsBlobClient downloadsBlobClient,
        IRoadNetworkExtractArchiveAssembler assembler,
        ILoggerFactory loggerFactory)
        : base(
            options,
            retryPolicy,
            ticketing,
            idempotentCommandHandler,
            roadRegistryContext,
            loggerFactory.CreateLogger<RequestExtractSqsLambdaRequestHandler>())
    {
        _wktReader = new WKTReader();
        _extractsDbContext = extractsDbContext;
        _downloadsBlobClient = downloadsBlobClient;
        _assembler = assembler;
    }

    protected override async Task<object> InnerHandle(RequestExtractSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        var extractRequestId = ExtractRequestId.FromString(request.Request.ExtractRequestId);
        var contour = _wktReader.Read(request.Request.Contour).ToMultiPolygon();
        var downloadId = new DownloadId(Guid.NewGuid());
        var extractDescription = new ExtractDescription(request.Request.Description);
        var isInformative = request.Request.IsInformative;

        var extractRequest = await _extractsDbContext.ExtractRequests.FindAsync([extractRequestId.ToString()], cancellationToken);
        if (extractRequest is null)
        {
            extractRequest = new ExtractRequest
            {
                ExtractRequestId = extractRequestId,
                OrganizationCode = request.Provenance.Operator,
                Description = extractDescription,
                ExternalRequestId = request.Request.ExternalRequestId,
                RequestedOn = DateTimeOffset.UtcNow,
                CurrentDownloadId = downloadId
            };
            _extractsDbContext.ExtractRequests.Add(extractRequest);
        }
        else
        {
            var existingOpenDownload = await _extractsDbContext.ExtractDownloads
                .SingleOrDefaultAsync(x => x.ExtractRequestId == extractRequestId
                                           && x.Closed == false, cancellationToken);
            if (existingOpenDownload is not null)
            {
                existingOpenDownload.Closed = true;
            }

            extractRequest.CurrentDownloadId = downloadId;
        }

        var extractDownload = new ExtractDownload
        {
            ExtractRequestId = extractRequestId,
            Contour = contour,
            IsInformative = isInformative,
            RequestedOn = extractRequest.RequestedOn,
            DownloadId = downloadId,
            TicketId = request.TicketId,
            DownloadStatus = ExtractDownloadStatus.Preparing
        };
        _extractsDbContext.ExtractDownloads.Add(extractDownload);

        await _extractsDbContext.SaveChangesAsync(cancellationToken);

        try
        {
            await BuildArchive(extractRequest, extractDownload, cancellationToken);

            extractDownload.DownloadStatus = ExtractDownloadStatus.Available;
        }
        catch
        {
            extractDownload.DownloadStatus = ExtractDownloadStatus.Error;
            throw;
        }
        finally
        {
            await _extractsDbContext.SaveChangesAsync(cancellationToken);
        }

        return new RequestExtractResponse(downloadId);
    }

    protected override Task ValidateIfMatchHeaderValue(RequestExtractSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task BuildArchive(
        ExtractRequest extractRequest,
        ExtractDownload extractDownload,
        CancellationToken ct)
    {
        var downloadId = new DownloadId(extractDownload.DownloadId);

        //TODO-pr nog nodig?
        // var overlappingDownloadIds = !message.Body.IsInformative
        //     ? await GetOverlappingDownloadIds(downloadId, message.Body.Contour, ct)
        //     : [];

        var request = new RoadNetworkExtractAssemblyRequest(
            default,
            downloadId,
            new ExtractDescription(extractRequest.Description),
            GeometryTranslator.Translate(GeometryTranslator.TranslateToRoadNetworkExtractGeometry((IPolygonal)extractDownload.Contour)),
            extractDownload.IsInformative,
            WellKnownZipArchiveWriterVersions.V2);

        try
        {
            using (var content = await _assembler.AssembleArchive(request, ct))
            {
                content.Position = 0L;

                await _downloadsBlobClient.CreateBlobAsync(
                    new BlobName(downloadId),
                    Metadata.None,
                    ContentType.Parse("application/x-zip-compressed"),
                    content,
                    ct);
            }
        }
        catch (SqlException ex) when (ex.Number.Equals(-2))
        {
            throw new ValidationException([
                new ValidationFailure
                {
                    PropertyName = "",
                    ErrorCode = "DatabaseTimeout", //TODO-pr use ProblemCode?
                    ErrorMessage = "Database timeout occurred while preparing the extract archive. "
                }
            ]);
        }
    }
}
