namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.RequestExtract;

using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using RoadRegistry.BackOffice.Abstractions.Extracts.V2;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.ValueObjects.ProblemCodes;

public class ExtractRequester
{
    private readonly ExtractsDbContext _extractsDbContext;
    private readonly RoadNetworkExtractDownloadsBlobClient _downloadsBlobClient;
    private readonly IRoadNetworkExtractArchiveAssembler _assembler;
    private readonly ILogger _logger;

    public ExtractRequester(
        ExtractsDbContext extractsDbContext,
        RoadNetworkExtractDownloadsBlobClient downloadsBlobClient,
        IRoadNetworkExtractArchiveAssembler assembler,
        ILoggerFactory loggerFactory)
    {
        _extractsDbContext = extractsDbContext;
        _downloadsBlobClient = downloadsBlobClient;
        _assembler = assembler;
        _logger = loggerFactory.CreateLogger<ExtractRequester>();
    }

    public async Task BuildExtract(
        RequestExtractData request,
        TicketId ticketId,
        string zipArchiveWriterVersion,
        Provenance provenance,
        Func<MemoryStream, Task>? onArchiveBuilt,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Building extract for ZipArchiveWriterVersion '{ZipArchiveWriterVersion}'", zipArchiveWriterVersion);

        var extractRequestId = request.ExtractRequestId;
        var downloadId = request.DownloadId;
        var contour = EnsureValidContour(request.Contour, downloadId);
        var extractDescription = new ExtractDescription(request.Description);
        var isInformative = request.IsInformative;

        var extractRequest = await _extractsDbContext.ExtractRequests.FindAsync([extractRequestId.ToString()], cancellationToken);
        if (extractRequest is null)
        {
            extractRequest = new ExtractRequest
            {
                ExtractRequestId = extractRequestId,
                OrganizationCode = provenance.Operator,
                Description = extractDescription,
                ExternalRequestId = request.ExternalRequestId,
                RequestedOn = DateTimeOffset.UtcNow,
                CurrentDownloadId = downloadId
            };
            _extractsDbContext.ExtractRequests.Add(extractRequest);
        }
        else if (extractRequest.CurrentDownloadId != downloadId)
        {
            var existingOpenDownloads = await _extractsDbContext.ExtractDownloads
                .Where(x => x.ExtractRequestId == extractRequestId && x.Closed == false && x.DownloadId != downloadId.ToGuid())
                .ToListAsync(cancellationToken);
            foreach (var existingOpenDownload in existingOpenDownloads)
            {
                existingOpenDownload.Close();
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
                RequestedOn = DateTimeOffset.UtcNow,
                DownloadId = downloadId,
                TicketId = ticketId,
                Status = ExtractDownloadStatus.Preparing,
                Closed = false,
                ZipArchiveWriterVersion = zipArchiveWriterVersion
            };
            _extractsDbContext.ExtractDownloads.Add(extractDownload);
        }

        await _extractsDbContext.SaveChangesAsync(cancellationToken);

        try
        {
            await using var archiveStream = await BuildArchive(extractRequest, extractDownload, cancellationToken);
            if (onArchiveBuilt is not null)
            {
                archiveStream.Position = 0;
                await onArchiveBuilt(archiveStream);
            }

            if (isInformative)
            {
                extractDownload.Close();
            }

            extractDownload.Status = ExtractDownloadStatus.Available;
        }
        catch
        {
            extractDownload.Status = ExtractDownloadStatus.Error;
            throw;
        }
        finally
        {
            await _extractsDbContext.SaveChangesAsync(cancellationToken);
        }
    }

    // Last line of defence before the contour is stored. The endpoints reject a contour the caller handed us, but one
    // we derived ourselves - a municipality boundary, a reprojected contour - never went through that, and
    // NetTopologySuite calling a geometry valid is no promise that SQL Server will. An invalid contour stored here
    // fails the spatial query that assembles this extract, and keeps failing the overlap check of every extract
    // requested after it, so it is corrected here rather than left to break both.
    private MultiPolygon EnsureValidContour(MultiPolygon contour, DownloadId downloadId)
    {
        if (contour.IsValid)
        {
            return contour;
        }

        // isKeepMulti the way every other contour correction in GeometryTranslator asks for it, so the result stays a
        // MultiPolygon even where the correction leaves a single ring. A contour that corrects to nothing at all is
        // not something we can store, and nothing downstream could make sense of it either.
        if (GeometryFixer.Fix(contour, isKeepMulti: true) is not MultiPolygon correctedContour || correctedContour.IsEmpty)
        {
            throw new ValidationException([
                new ValidationFailure
                {
                    PropertyName = nameof(RequestExtractData.Contour),
                    ErrorCode = ProblemCode.Extract.ContourInvalid
                }
            ]);
        }

        _logger.LogWarning(
            "Contour of download {DownloadId} was not a valid geometry and has been corrected, its area went from {OriginalArea} to {CorrectedArea}",
            downloadId,
            contour.Area,
            correctedContour.Area);

        return correctedContour;
    }

    private async Task<MemoryStream> BuildArchive(
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
            _logger.LogInformation("Starting AssembleArchive on type '{AssemblerType}' for ZipArchiveWriterVersion '{ZipArchiveWriterVersion}'", _assembler.GetType().FullName, request.ZipArchiveWriterVersion);
            var content = await _assembler.AssembleArchive(request, ct);
            content.Position = 0L;

            await _downloadsBlobClient.CreateBlobAsync(
                new BlobName(downloadId),
                Metadata.None,
                ContentType.Parse("application/x-zip-compressed"),
                content,
                ct);

            return content;
        }
        catch (SqlException ex) when (ex.Number.Equals(-2))
        {
            _logger.LogError(ex, $"Database timeout while creating extract for download {downloadId}: {ex.Message}");

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
