namespace RoadRegistry.BackOffice.Extracts
{
    using System;
    using Core;
    using Framework;
    using Messages;
    using NodaTime;
    using SqlStreamStore;

    public class RoadNetworkExtractCommandModule : CommandHandlerModule
    {
        public RoadNetworkExtractCommandModule(IStreamStore store, IRoadNetworkSnapshotReader snapshotReader, IClock clock)
        {
            if (store == null) throw new ArgumentNullException(nameof(store));
            if (snapshotReader == null) throw new ArgumentNullException(nameof(snapshotReader));
            if (clock == null) throw new ArgumentNullException(nameof(clock));

            For<RequestRoadNetworkExtract>()
                .UseValidator(new RequestRoadNetworkExtractValidator())
                .UseRoadRegistryContext(store, snapshotReader, EnrichEvent.WithTime(clock))
                .Handle(async (context, message, ct) =>
                {
                    var externalRequestId = new ExternalExtractRequestId(message.Body.ExternalRequestId);
                    var requestId = ExtractRequestId.FromExternalRequestId(externalRequestId);
                    var downloadId = new DownloadId(message.Body.DownloadId);
                    var contour = GeometryTranslator.Translate(message.Body.Contour);
                    var extract = await context.RoadNetworkExtracts.Get(requestId, ct);
                    if (extract == null)
                    {
                        extract = RoadNetworkExtract.Request(externalRequestId, downloadId, contour);
                        context.RoadNetworkExtracts.Add(extract);
                    }
                    else
                    {
                        extract.RequestAgain(downloadId, contour);
                    }
                });

            For<AnnounceRoadNetworkExtractDownloadBecameAvailable>()
                .UseRoadRegistryContext(store, snapshotReader, EnrichEvent.WithTime(clock))
                .Handle(async (context, message, ct) =>
                {
                    var requestId = ExtractRequestId.FromString(message.Body.RequestId);
                    var downloadId = new DownloadId(message.Body.DownloadId);
                    var archiveId = new ArchiveId(message.Body.ArchiveId);
                    var extract = await context.RoadNetworkExtracts.Get(requestId, ct);
                    extract.Announce(downloadId, archiveId);
                });
        }
    }
}
