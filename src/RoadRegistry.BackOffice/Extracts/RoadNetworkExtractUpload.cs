namespace RoadRegistry.BackOffice.Extracts;

using System;
using System.IO.Compression;
using System.Linq;
using Messages;
using Uploads;

public class RoadNetworkExtractUpload
{
    private readonly Action<object> _applier;
    private readonly ArchiveId _archiveId;
    private readonly DownloadId _downloadId;
    private readonly ExternalExtractRequestId _externalRequestId;
    private readonly ExtractRequestId _requestId;
    private readonly ExtractDescription _extractDescription;
    private readonly UploadId _uploadId;

    internal RoadNetworkExtractUpload(ExternalExtractRequestId externalRequestId, ExtractRequestId requestId, ExtractDescription extractDescription, DownloadId downloadId, UploadId uploadId, ArchiveId archiveId, Action<object> applier)
    {
        _externalRequestId = externalRequestId;
        _requestId = requestId;
        _extractDescription = extractDescription;
        _downloadId = downloadId;
        _uploadId = uploadId;
        _archiveId = archiveId;
        _applier = applier;
    }

    private void Apply(object @event)
    {
        _applier(@event);
    }

    public ZipArchiveProblems ValidateArchiveUsing(ZipArchive archive, IZipArchiveValidator validator, bool useZipArchiveFeatureCompareTranslator = false)
    {
        var zipArchiveMetadata = ZipArchiveMetadata.Empty.WithDownloadId(_downloadId);

        var problems = validator.Validate(archive, zipArchiveMetadata);
        if (!problems.OfType<FileError>().Any())
            Apply(
                new RoadNetworkExtractChangesArchiveAccepted
                {
                    Description = _extractDescription,
                    ExternalRequestId = _externalRequestId,
                    RequestId = _requestId,
                    DownloadId = _downloadId,
                    UploadId = _uploadId,
                    ArchiveId = _archiveId,
                    Problems = problems.Select(problem => problem.Translate()).ToArray(),
                    UseZipArchiveFeatureCompareTranslator = useZipArchiveFeatureCompareTranslator
                });
        else
            Apply(
                new RoadNetworkExtractChangesArchiveRejected
                {
                    Description = _extractDescription,
                    ExternalRequestId = _externalRequestId,
                    RequestId = _requestId,
                    DownloadId = _downloadId,
                    UploadId = _uploadId,
                    ArchiveId = _archiveId,
                    Problems = problems.Select(problem => problem.Translate()).ToArray()
                });

        return problems;
    }
}
