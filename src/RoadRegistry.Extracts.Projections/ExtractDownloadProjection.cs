namespace RoadRegistry.Extracts.Projections;

using System.Linq;
using BackOffice;
using BackOffice.Extensions;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using NetTopologySuite.Geometries;
using NodaTime.Text;
using Schema;

public class ExtractDownloadProjection : ConnectedProjection<ExtractsDbContext>
{
    public ExtractDownloadProjection()
    {
        When<Envelope<RoadNetworkExtractGotRequested>>(async (context, envelope, ct) =>
        {
            var record = await context.ExtractDownloads.IncludeLocalSingleOrDefaultAsync(x => x.DownloadId == envelope.Message.DownloadId, ct);
            if (record is null)
            {
                record = new ExtractDownload
                {
                    DownloadId = envelope.Message.DownloadId,
                    ExtractRequestId = ExtractRequestId.FromExternalRequestId(new ExternalExtractRequestId(envelope.Message.ExternalRequestId)),
                    Contour = (Geometry)GeometryTranslator.Translate(envelope.Message.Contour),
                    IsInformative = envelope.Message.IsInformative,
                    RequestedOn = InstantPattern.ExtendedIso.Parse(envelope.Message.When).Value.ToDateTimeOffset(),
                    DownloadedOn = null,
                    Closed = false,
                    DownloadStatus = ExtractDownloadStatus.Preparing
                };
                await context.ExtractDownloads.AddAsync(record, ct);
            }
        });

        When<Envelope<RoadNetworkExtractGotRequestedV2>>(async (context, envelope, ct) =>
        {
            var record = await context.ExtractDownloads.IncludeLocalSingleOrDefaultAsync(x => x.DownloadId == envelope.Message.DownloadId, ct);
            if (record is null)
            {
                record = new ExtractDownload
                {
                    DownloadId = envelope.Message.DownloadId,
                    ExtractRequestId = ExtractRequestId.FromExternalRequestId(new ExternalExtractRequestId(envelope.Message.ExternalRequestId)),
                    Contour = (Geometry)GeometryTranslator.Translate(envelope.Message.Contour),
                    IsInformative = envelope.Message.IsInformative,
                    RequestedOn = InstantPattern.ExtendedIso.Parse(envelope.Message.When).Value.ToDateTimeOffset(),
                    DownloadedOn = null,
                    Closed = false,
                    DownloadStatus = ExtractDownloadStatus.Preparing
                };
                await context.ExtractDownloads.AddAsync(record, ct);
            }
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

            records.ForEach(record => record.Closed = true);
        });

        When<Envelope<RoadNetworkExtractDownloadBecameAvailable>>(async (context, envelope, ct) =>
        {
            var record = await context.ExtractDownloads.IncludeLocalSingleAsync(download => download.DownloadId == envelope.Message.DownloadId, ct);
            record.DownloadStatus = ExtractDownloadStatus.Available;
            if (record.IsInformative)
            {
                record.Closed = true;
            }
        });

        When<Envelope<RoadNetworkExtractDownloadTimeoutOccurred>>(async (context, envelope, ct) =>
        {
            if (envelope.Message.DownloadId is null)
            {
                return;
            }

            var record = await context.ExtractDownloads.IncludeLocalSingleAsync(download => download.DownloadId == envelope.Message.DownloadId.Value, ct);
            record.DownloadStatus = ExtractDownloadStatus.Error;
        });

        When<Envelope<RoadNetworkExtractChangesArchiveUploaded>>(async (context, envelope, ct) =>
        {
            var record = await context.ExtractDownloads.IncludeLocalSingleAsync(download => download.DownloadId == envelope.Message.DownloadId, ct);
            record.UploadId = envelope.Message.UploadId;
            record.UploadedOn = InstantPattern.ExtendedIso.Parse(envelope.Message.When).Value.ToDateTimeOffset();
            record.UploadStatus = ExtractUploadStatus.Processing;
            record.TicketId = envelope.Message.TicketId;
        });

        When<Envelope<RoadNetworkExtractChangesArchiveAccepted>>(async (context, envelope, ct) =>
        {
            var record = await context.ExtractDownloads.IncludeLocalSingleAsync(download => download.DownloadId == envelope.Message.DownloadId, ct);
            record.UploadStatus = ExtractUploadStatus.Accepted;
            record.TicketId = envelope.Message.TicketId;
        });

        When<Envelope<RoadNetworkExtractChangesArchiveRejected>>(async (context, envelope, ct) =>
        {
            var record = await context.ExtractDownloads.IncludeLocalSingleAsync(download => download.DownloadId == envelope.Message.DownloadId, ct);
            record.UploadStatus = ExtractUploadStatus.Rejected;
        });

        When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, ct) =>
        {
            var record = await context.ExtractDownloads.IncludeLocalSingleOrDefaultAsync(download => download.DownloadId == envelope.Message.DownloadId, ct);
            if (record is not null)
            {
                record.UploadStatus = ExtractUploadStatus.Accepted;
            }
        });

        When<Envelope<RoadNetworkChangesRejected>>(async (context, envelope, ct) =>
        {
            ExtractDownload record = null;

            if (envelope.Message.TicketId is not null)
            {
                record = await context.ExtractDownloads.IncludeLocalSingleOrDefaultAsync(download => download.TicketId == envelope.Message.TicketId, ct);
            }
            else
            {
                // grb uploads
                var extractRequest = await context.ExtractRequests.IncludeLocalSingleOrDefaultAsync(request => request.ExternalRequestId == envelope.Message.Reason, ct);
                if (extractRequest is not null)
                {
                    record = await context.ExtractDownloads.IncludeLocalSingleOrDefaultAsync(download => download.DownloadId == extractRequest.CurrentDownloadId, ct);
                }
            }

            if (record is not null)
            {
                record.UploadStatus = ExtractUploadStatus.Rejected;
            }
        });

        When<Envelope<NoRoadNetworkChanges>>(async (context, envelope, ct) =>
        {
            var record = await context.ExtractDownloads.IncludeLocalSingleOrDefaultAsync(download => download.DownloadId == envelope.Message.DownloadId, ct);
            if (record is not null)
            {
                record.UploadStatus = ExtractUploadStatus.Accepted;
            }
        });
    }
}
