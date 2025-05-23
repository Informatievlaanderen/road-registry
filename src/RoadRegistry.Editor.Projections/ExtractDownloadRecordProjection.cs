namespace RoadRegistry.Editor.Projections;

using BackOffice;
using BackOffice.Extensions;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using NodaTime.Text;
using Schema;
using Schema.Extracts;
using System.Linq;
using NodaTime;

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
            var record = await context.ExtractDownloads.IncludeLocalSingleAsync(download => download.DownloadId == envelope.Message.DownloadId, ct);
            record.DownloadedOn = InstantPattern.ExtendedIso.Parse(envelope.Message.When).Value.ToDateTimeOffset();
        });

        When<Envelope<RoadNetworkExtractClosed>>(async (context, envelope, ct) =>
        {
            var downloadIds = envelope.Message.DownloadIds.Select(x => DownloadId.Parse(x).ToGuid()).ToArray();

            var records = await context.ExtractDownloads
                .IncludeLocalToListAsync(q => q.Where(record => downloadIds.Contains(record.DownloadId)), ct);

            records.ForEach(record => record.IsInformative = true);
        });

        When<Envelope<RoadNetworkExtractDownloadBecameAvailable>>(async (context, envelope, ct) =>
        {
            var record = await context.ExtractDownloads.IncludeLocalSingleAsync(download => download.DownloadId == envelope.Message.DownloadId, ct);
            record.ArchiveId = envelope.Message.ArchiveId;
            record.Available = true;
            record.AvailableOn = InstantPattern.ExtendedIso.Parse(envelope.Message.When).Value.ToUnixTimeSeconds();
            record.IsInformative = envelope.Message.IsInformative;
            record.ZipArchiveWriterVersion = envelope.Message.ZipArchiveWriterVersion;
        });

        When<Envelope<RoadNetworkExtractDownloadTimeoutOccurred>>(async (context, envelope, ct) =>
        {
            ExtractDownloadRecord record;

            if (envelope.Message.DownloadId is not null)
            {
                record = await context.ExtractDownloads.IncludeLocalSingleAsync(download => download.DownloadId == envelope.Message.DownloadId.Value, ct);
            }
            else
            {
                record = await context.ExtractDownloads.IncludeLocalSingleAsync(download => download.RequestId == envelope.Message.RequestId && download.Available == false, ct);
            }

            record.Available = true;
            record.AvailableOn = InstantPattern.ExtendedIso.Parse(envelope.Message.When).Value.ToUnixTimeSeconds();
            record.IsInformative = envelope.Message.IsInformative;
        });
    }
}
