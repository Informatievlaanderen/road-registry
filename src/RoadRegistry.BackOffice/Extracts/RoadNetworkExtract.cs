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
        private ExtractDescription _extractDescription;

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
                _extractDescription = new ExtractDescription(string.Empty);
                _requestedDownloads.Add(new DownloadId(e.DownloadId));
            });
            On<RoadNetworkExtractGotRequestedV2>(e =>
            {
                Id = ExtractRequestId.FromString(e.RequestId);
                _externalExtractRequestId = new ExternalExtractRequestId(e.ExternalRequestId);
                _extractDescription = new ExtractDescription(e.Description);
                _requestedDownloads.Add(new DownloadId(e.DownloadId));
            });
            On<RoadNetworkExtractDownloadBecameAvailable>(e =>
            {
                _announcedDownloads.Add(new DownloadId(e.DownloadId));
            });
            On<RoadNetworkExtractChangesArchiveUploaded>(e =>
            {
                _knownUploads.Add(new UploadId(e.UploadId));
            });
        }

        public ExtractRequestId Id { get; private set; }

        public static RoadNetworkExtract Request(
            ExternalExtractRequestId externalExtractRequestId,
            DownloadId downloadId,
            ExtractDescription extractDescription,
            IPolygonal contour)
        {
            var instance = Factory();
            instance.Apply(new RoadNetworkExtractGotRequestedV2
            {
                RequestId = ExtractRequestId.FromExternalRequestId(externalExtractRequestId).ToString(),
                ExternalRequestId = externalExtractRequestId,
                DownloadId = downloadId,
                Description = extractDescription,
                Contour = GeometryTranslator.TranslateToRoadNetworkExtractGeometry(contour)
            });
            return instance;
        }

        public void RequestAgain(DownloadId downloadId, IPolygonal contour)
        {
            if (!_requestedDownloads.Contains(downloadId))
            {
                Apply(new RoadNetworkExtractGotRequestedV2
                {
                    RequestId = Id.ToString(),
                    ExternalRequestId = _externalExtractRequestId,
                    Description = _extractDescription,
                    DownloadId = downloadId,
                    Contour = GeometryTranslator.TranslateToRoadNetworkExtractGeometry(contour)
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
                    Description = _extractDescription,
                    DownloadId = downloadId,
                    ArchiveId = archiveId
                });
            }
        }

        public RoadNetworkExtractUpload Upload(DownloadId downloadId, UploadId uploadId, ArchiveId archiveId)
        {
            if (!_requestedDownloads.Contains(downloadId))
            {
                throw new CanNotUploadRoadNetworkExtractChangesArchiveForUnknownDownload(
                    _externalExtractRequestId, Id, downloadId, uploadId);
            }

            if (_requestedDownloads[_requestedDownloads.Count - 1] != downloadId)
            {
                throw new CanNotUploadRoadNetworkExtractChangesArchiveForSupersededDownload(
                    _externalExtractRequestId, Id, downloadId, _requestedDownloads[_requestedDownloads.Count - 1], uploadId);
            }

            if (_knownUploads.Count == 1)
            {
                throw new CanNotUploadRoadNetworkExtractChangesArchiveForSameDownloadMoreThanOnce(
                    _externalExtractRequestId, Id, downloadId, uploadId);
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
