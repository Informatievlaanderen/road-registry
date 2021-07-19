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

        private readonly HashSet<DownloadId> _requestedDownloads;
        private readonly HashSet<DownloadId> _announcedDownloads;

        private RoadNetworkExtract()
        {
            _requestedDownloads = new HashSet<DownloadId>();
            _announcedDownloads = new HashSet<DownloadId>();

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
            IPolygonal contour)
        {
            var instance = Factory();
            instance.Apply(new RoadNetworkExtractGotRequested
            {
                RequestId = ExtractRequestId.FromExternalRequestId(externalExtractRequestId).ToString(),
                ExternalRequestId = externalExtractRequestId,
                DownloadId = downloadId,
                Contour = GeometryTranslator.TranslateToRoadNetworkExtractGeometry(contour)
            });
            return instance;
        }

        public void RequestAgain(DownloadId downloadId, IPolygonal contour)
        {
            if (!_requestedDownloads.Contains(downloadId))
            {
                Apply(new RoadNetworkExtractGotRequested
                {
                    RequestId = Id.ToString(),
                    ExternalRequestId = _externalExtractRequestId,
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
                    DownloadId = downloadId,
                    ArchiveId = archiveId
                });
            }
        }
    }
}
