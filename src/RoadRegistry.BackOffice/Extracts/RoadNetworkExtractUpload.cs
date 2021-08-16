namespace RoadRegistry.BackOffice.Extracts
{
    using System;
    using System.IO.Compression;
    using System.Linq;
    using Messages;
    using Uploads;

    public class RoadNetworkExtractUpload
    {
        private readonly ExternalExtractRequestId _externalRequestId;
        private readonly ExtractRequestId _requestId;
        private readonly DownloadId _downloadId;
        private readonly UploadId _uploadId;
        private readonly ArchiveId _archiveId;
        private readonly Action<object> _applier;

        internal RoadNetworkExtractUpload(ExternalExtractRequestId externalRequestId, ExtractRequestId requestId, DownloadId downloadId, UploadId uploadId, ArchiveId archiveId, Action<object> applier)
        {
            _externalRequestId = externalRequestId;
            _requestId = requestId;
            _downloadId = downloadId;
            _uploadId = uploadId;
            _archiveId = archiveId;
            _applier = applier;
        }

        private void Apply(object @event) => _applier(@event);

        public void ValidateArchiveUsing(ZipArchive archive, IZipArchiveValidator validator)
        {
            var problems = validator.Validate(archive);
            if (!problems.OfType<FileError>().Any())
            {
                Apply(
                    new RoadNetworkExtractChangesArchiveAccepted
                    {
                        ExternalRequestId = _externalRequestId,
                        RequestId = _requestId,
                        DownloadId = _downloadId,
                        UploadId = _uploadId,
                        ArchiveId = _archiveId,
                        Problems = problems.Select(problem => problem.Translate()).ToArray()
                    });
            }
            else
            {
                Apply(
                    new RoadNetworkExtractChangesArchiveRejected
                    {
                        ExternalRequestId = _externalRequestId,
                        RequestId = _requestId,
                        DownloadId = _downloadId,
                        UploadId = _uploadId,
                        ArchiveId = _archiveId,
                        Problems = problems.Select(problem => problem.Translate()).ToArray()
                    });
            }
        }
    }
}
