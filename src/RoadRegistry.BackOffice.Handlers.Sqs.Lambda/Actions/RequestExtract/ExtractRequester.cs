namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.RequestExtract;

using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice.Abstractions.Extracts.V2;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.FeatureToggles;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.Schema;

public class ExtractRequester
{
    private readonly ExtractsDbContext _extractsDbContext;
    private readonly RoadNetworkExtractDownloadsBlobClient _downloadsBlobClient;
    private readonly IRoadNetworkExtractArchiveAssembler _assembler;
    private readonly UseDomainV2FeatureToggle _useDomainV2FeatureToggle;
    private readonly ILogger _logger;

    public ExtractRequester(
        ExtractsDbContext extractsDbContext,
        RoadNetworkExtractDownloadsBlobClient downloadsBlobClient,
        IRoadNetworkExtractArchiveAssembler assembler,
        UseDomainV2FeatureToggle useDomainV2FeatureToggle,
        ILoggerFactory loggerFactory)
    {
        _extractsDbContext = extractsDbContext;
        _downloadsBlobClient = downloadsBlobClient;
        _assembler = assembler;
        _useDomainV2FeatureToggle = useDomainV2FeatureToggle;
        _logger = loggerFactory.CreateLogger<ExtractRequester>();
    }

    public async Task BuildExtract(RequestExtractData request, TicketId ticketId, Provenance provenance, CancellationToken cancellationToken)
    {
        var extractRequestId = request.ExtractRequestId;
        var contour = request.Contour;
        var downloadId = request.DownloadId;
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
                TicketId = ticketId,
                DownloadStatus = ExtractDownloadStatus.Preparing,
                Closed = isInformative,
                ZipArchiveWriterVersion = _useDomainV2FeatureToggle.FeatureEnabled
                    ? WellKnownZipArchiveWriterVersions.DomainV2
                    : WellKnownZipArchiveWriterVersions.V2
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
