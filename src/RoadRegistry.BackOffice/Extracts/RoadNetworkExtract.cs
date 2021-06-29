namespace RoadRegistry.BackOffice.Extracts
{
    using System;
    using Framework;
    using Messages;
    using NetTopologySuite.Geometries;

    public class RoadNetworkExtract : EventSourcedEntity
    {
        public static readonly Func<RoadNetworkExtract> Factory = () => new RoadNetworkExtract();

        private ExternalExtractRequestId _externalExtractRequestId;

        private RoadNetworkExtract()
        {
            On<RoadNetworkExtractGotRequested>(e =>
            {
                Id = ExtractRequestId.FromString(e.RequestId);
                _externalExtractRequestId = new ExternalExtractRequestId(e.ExternalRequestId);
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
            Apply(new RoadNetworkExtractGotRequested
            {
                RequestId = Id.ToString(),
                ExternalRequestId = _externalExtractRequestId,
                DownloadId = downloadId,
                Contour = GeometryTranslator.TranslateToRoadNetworkExtractGeometry(boundary)
            });
        }
    }
}
