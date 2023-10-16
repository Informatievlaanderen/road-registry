namespace RoadRegistry.Editor.Projections;

using System.Linq;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Microsoft.EntityFrameworkCore;
using NodaTime.Text;
using RoadRegistry.BackOffice;
using Schema;
using Schema.Extracts;

public class ExtractDownloadRecordProjection : ConnectedProjection<EditorContext>
{
    public ExtractDownloadRecordProjection()
    {
        When<Envelope<RoadNetworkExtractGotRequested>>(async (context, envelope, ct) =>
        {
            var record = new ExtractDownloadRecord
            {
                ExternalRequestId = envelope.Message.ExternalRequestId,
                RequestId = envelope.Message.RequestId,
                DownloadId = envelope.Message.DownloadId,
                ArchiveId = null,
                Available = false,
                AvailableOn = 0L,
                RequestedOn = InstantPattern.ExtendedIso.Parse(envelope.Message.When).Value.ToUnixTimeSeconds(),
                IsInformative = envelope.Message.IsInformative
            };
            await context.ExtractDownloads.AddAsync(record, ct);
        });

        When<Envelope<RoadNetworkExtractGotRequestedV2>>(async (context, envelope, ct) =>
        {
            var record = new ExtractDownloadRecord
            {
                ExternalRequestId = envelope.Message.ExternalRequestId,
                RequestId = envelope.Message.RequestId,
                DownloadId = envelope.Message.DownloadId,
                ArchiveId = null,
                Available = false,
                AvailableOn = 0L,
                RequestedOn = InstantPattern.ExtendedIso.Parse(envelope.Message.When).Value.ToUnixTimeSeconds(),
                IsInformative = envelope.Message.IsInformative
            };
            await context.ExtractDownloads.AddAsync(record, ct);
        });

        When<Envelope<RoadNetworkExtractDownloaded>>(async (context, envelope, ct) =>
        {
            var record =
                context.ExtractDownloads.Local.SingleOrDefault(download => download.DownloadId == envelope.Message.DownloadId)
                ?? await context.ExtractDownloads.SingleAsync(download => download.DownloadId == envelope.Message.DownloadId, ct);
            record.DownloadedOn = InstantPattern.ExtendedIso.Parse(envelope.Message.When).Value.ToDateTimeOffset();
        });

        When<Envelope<RoadNetworkExtractClosed>>(async (context, envelope, ct) =>
        {
            var downloadIds = envelope.Message.DownloadIds.Select(x => DownloadId.Parse(x).ToGuid()).ToArray();

            var records = context.ExtractDownloads.Local
                    .Where(record => downloadIds.Contains(record.DownloadId))
                    .ToList()
                    .Concat(await context.ExtractDownloads
                        .Where(record => downloadIds.Contains(record.DownloadId))
                        .ToListAsync(ct))
                    .ToList();

            records.ForEach(record => record.IsInformative = true);
        });

        When<Envelope<RoadNetworkExtractDownloadBecameAvailable>>(async (context, envelope, ct) =>
        {
            var record =
                context.ExtractDownloads.Local.SingleOrDefault(download => download.DownloadId == envelope.Message.DownloadId)
                ?? await context.ExtractDownloads.SingleAsync(download => download.DownloadId == envelope.Message.DownloadId, ct);
            record.ArchiveId = envelope.Message.ArchiveId;
            record.Available = true;
            record.AvailableOn = InstantPattern.ExtendedIso.Parse(envelope.Message.When).Value.ToUnixTimeSeconds();
            record.IsInformative = envelope.Message.IsInformative;
        });

        When<Envelope<RoadNetworkExtractDownloadTimeoutOccurred>>(async (context, envelope, ct) =>
        {
            var record =
                context.ExtractDownloads.Local.SingleOrDefault(download => download.RequestId == envelope.Message.RequestId)
                ?? await context.ExtractDownloads.SingleAsync(download => download.RequestId == envelope.Message.RequestId, ct);
            record.Available = true;
            record.AvailableOn = InstantPattern.ExtendedIso.Parse(envelope.Message.When).Value.ToUnixTimeSeconds();
            record.IsInformative = envelope.Message.IsInformative;
        });
    }
}
