namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers.Extracts;

using Abstractions.Extracts.V2;
using BackOffice.Extracts;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
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
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.Schema;
using TicketingService.Abstractions;

public sealed class RequestInwinningExtractSqsLambdaRequestHandler : SqsLambdaHandler<RequestInwinningExtractSqsLambdaRequest>
{
    private readonly WKTReader _wktReader;
    private readonly ExtractsDbContext _extractsDbContext;
    private readonly RoadNetworkExtractDownloadsBlobClient _downloadsBlobClient;
    private readonly IRoadNetworkExtractArchiveAssembler _assembler;

    public RequestInwinningExtractSqsLambdaRequestHandler(
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
            loggerFactory.CreateLogger<RequestInwinningExtractSqsLambdaRequestHandler>())
    {
        _wktReader = new WKTReader();
        _extractsDbContext = extractsDbContext;
        _downloadsBlobClient = downloadsBlobClient;
        _assembler = assembler;
    }

    protected override async Task<object> InnerHandle(RequestInwinningExtractSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        var niscode = request.Request.NisCode;
        var inwinningsZone = await _extractsDbContext.Inwinningszones.FindAsync([niscode], cancellationToken);
        if (inwinningsZone is not null)
        {
            //TODO-pr wat als er al een inwinning is gestart? dan zou er ook nog geen extractRequest mogen bestaan (logica mee aanpassen)
            throw new ValidationException([
                new ValidationFailure
                {
                    PropertyName = string.Empty,
                    ErrorCode = "Inwinningszone",
                    ErrorMessage = $"Inwinning is al bezig voor {niscode}."
                }
            ]);
        }

        var contour = _wktReader.Read(request.Request.Contour).ToMultiPolygon();
        // ensure SRID is filled in
        contour = (MultiPolygon)GeometryTranslator.Translate(GeometryTranslator.TranslateToRoadNetworkExtractGeometry(contour));

        inwinningsZone = new Inwinningszone
        {
            NisCode = niscode,
            Contour = contour,
            Completed = false
        };
        _extractsDbContext.Inwinningszones.Add(inwinningsZone);

        var extractRequestId = ExtractRequestId.FromString(request.Request.ExtractRequestId);
        var downloadId = new DownloadId(request.Request.DownloadId);
        var extractDescription = new ExtractDescription($"Data-inwinning {niscode}");
        var isInformative = false;

        var extractRequest = await _extractsDbContext.ExtractRequests.FindAsync([extractRequestId.ToString()], cancellationToken);
        if (extractRequest is null)
        {
            extractRequest = new ExtractRequest
            {
                ExtractRequestId = extractRequestId,
                OrganizationCode = request.Provenance.Operator,
                Description = extractDescription,
                ExternalRequestId = null,
                RequestedOn = DateTimeOffset.UtcNow,
                CurrentDownloadId = downloadId
            };
            _extractsDbContext.ExtractRequests.Add(extractRequest);
        }
        else if (extractRequest.CurrentDownloadId != downloadId)
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

        var extractDownload = await _extractsDbContext.ExtractDownloads.FindAsync([downloadId.ToGuid()], cancellationToken);
        if (extractDownload is null)
        {
            extractDownload = new ExtractDownload
            {
                ExtractRequestId = extractRequestId,
                Contour = contour,
                IsInformative = isInformative,
                RequestedOn = extractRequest.RequestedOn,
                DownloadId = downloadId,
                TicketId = request.TicketId,
                DownloadStatus = ExtractDownloadStatus.Preparing,
                Closed = isInformative,
                ZipArchiveWriterVersion = WellKnownZipArchiveWriterVersions.DomainV2
            };
            _extractsDbContext.ExtractDownloads.Add(extractDownload);
        }

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

    protected override Task ValidateIfMatchHeaderValue(RequestInwinningExtractSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task BuildArchive(
        ExtractRequest extractRequest,
        ExtractDownload extractDownload,
        CancellationToken ct)
    {
        var downloadId = new DownloadId(extractDownload.DownloadId);

        var request = new RoadNetworkExtractAssemblyRequest(
            downloadId,
            new ExtractDescription(extractRequest.Description),
            (IPolygonal)extractDownload.Contour,
            extractDownload.IsInformative,
            extractDownload.ZipArchiveWriterVersion);

        try
        {
            using var content = await _assembler.AssembleArchive(request, ct);
            content.Position = 0L;

            await _downloadsBlobClient.CreateBlobAsync(
                new BlobName(downloadId),
                Metadata.None,
                ContentType.Parse("application/x-zip-compressed"),
                content,
                ct);
        }
        catch (SqlException ex) when (ex.Number.Equals(-2))
        {
            Logger.LogError(ex, $"Database timeout while creating extract for download {downloadId}: {ex.Message}");

            throw new ValidationException([
                new ValidationFailure
                {
                    PropertyName = string.Empty,
                    ErrorCode = "DatabaseTimeout",
                    ErrorMessage = "Er was een probleem met de databank tijdens het aanmaken van het extract."
                }
            ]);
        }
    }
}
