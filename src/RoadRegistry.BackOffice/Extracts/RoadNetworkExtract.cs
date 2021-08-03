namespace RoadRegistry.BackOffice.Extracts
{
    using System;
    using System.Collections.Generic;
    using Framework;
    using Messages;
    using NetTopologySuite.Geometries;

    public class RoadNetworkExtract : EventSourcedEntity
    {
        public static readonly Func<RoadNetworkExtract> Factory = () => new RoadNetworkExtract();

        private ExternalExtractRequestId _externalExtractRequestId;

        private readonly List<DownloadId> _requestedDownloads;
        private readonly HashSet<DownloadId> _announcedDownloads;
        private readonly HashSet<UploadId> _knownUploads;

        private RoadNetworkExtract()
        {
            _requestedDownloads = new List<DownloadId>();
            _announcedDownloads = new HashSet<DownloadId>();
            _knownUploads = new HashSet<UploadId>();

            On<RoadNetworkExtractGotRequested>(e =>
            {
                Id = ExtractRequestId.FromString(e.RequestId);
                _externalExtractRequestId = new ExternalExtractRequestId(e.ExternalRequestId);
                _requestedDownloads.Add(new DownloadId(e.DownloadId));
            });
            On<RoadNetworkExtractDownloadBecameAvailable>(e =>
            {
                _announcedDownloads.Add(new DownloadId(e.DownloadId));
            });
        }

        public ExtractRequestId Id { get; private set; }

        public static RoadNetworkExtract Request(
            ExternalExtractRequestId externalExtractRequestId,
            DownloadId downloadId,
            MultiPolygon boundary)
        {
            var instance = Factory();
            instance.Apply(new RoadNetworkExtractGotRequested
            {
                RequestId = ExtractRequestId.FromExternalRequestId(externalExtractRequestId).ToString(),
                ExternalRequestId = externalExtractRequestId,
                DownloadId = downloadId,
                Contour = GeometryTranslator.TranslateToRoadNetworkExtractGeometry(boundary)
            });
            return instance;
        }

        public void RequestAgain(DownloadId downloadId, MultiPolygon boundary)
        {
            if (!_requestedDownloads.Contains(downloadId))
            {
                Apply(new RoadNetworkExtractGotRequested
                {
                    RequestId = Id.ToString(),
                    ExternalRequestId = _externalExtractRequestId,
                    DownloadId = downloadId,
                    Contour = GeometryTranslator.TranslateToRoadNetworkExtractGeometry(boundary)
                });
            }
        }

        public void Announce(DownloadId downloadId, ArchiveId archiveId)
        {
            if (_requestedDownloads.Contains(downloadId) && !_announcedDownloads.Contains(downloadId))
            {
                Apply(new RoadNetworkExtractDownloadBecameAvailable
                {
                    RequestId = Id.ToString(),
                    ExternalRequestId = _externalExtractRequestId,
                    DownloadId = downloadId,
                    ArchiveId = archiveId
                });
            }
        }

        public RoadNetworkExtractUpload Upload(DownloadId downloadId, UploadId uploadId, ArchiveId archiveId)
        {
            if (!_requestedDownloads.Contains(downloadId))
            {
                // TODO: Not sure how you got here but you can't upload for a download we don't know about
            }

            if (_requestedDownloads[_requestedDownloads.Count - 1] != downloadId)
            {
                // TODO: You can only upload for the last requested download, not for an older version
            }

            if (_knownUploads.Contains(uploadId))
            {
                // TODO: Not sure how you got here but you can only upload the same upload id once
            }

            Apply(new RoadNetworkExtractChangesArchiveUploaded
            {
                RequestId = Id,
                ExternalRequestId = _externalExtractRequestId,
                DownloadId = downloadId,
                UploadId = uploadId,
                ArchiveId = archiveId
            });

            return new RoadNetworkExtractUpload(
                _externalExtractRequestId,
                Id,
                downloadId,
                uploadId,
                archiveId,
                Apply);
        }
    }
}
