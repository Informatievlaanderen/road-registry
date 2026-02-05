namespace RoadRegistry.Extracts.Projections;

using System;
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
                    Status = ExtractDownloadStatus.Preparing
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
                    Status = ExtractDownloadStatus.Preparing
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
            record.Status = ExtractDownloadStatus.Available;
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
            record.Status = ExtractDownloadStatus.Error;
        });

        When<Envelope<RoadNetworkExtractChangesArchiveUploaded>>(async (context, envelope, ct) =>
        {
            var download = await context.ExtractDownloads.IncludeLocalSingleAsync(download => download.DownloadId == envelope.Message.DownloadId, ct);
            download.LatestUploadId = envelope.Message.UploadId;

            var upload = new ExtractUpload
            {
                UploadId = envelope.Message.UploadId,
                DownloadId = download.DownloadId,
                Status = ExtractUploadStatus.Processing,
                UploadedOn = InstantPattern.ExtendedIso.Parse(envelope.Message.When).Value.ToDateTimeOffset(),
                TicketId = envelope.Message.TicketId ?? Guid.Empty
            };
            context.ExtractUploads.Add(upload);
        });

        When<Envelope<RoadNetworkExtractChangesArchiveAccepted>>(async (context, envelope, ct) =>
        {
            var record = await context.ExtractUploads.IncludeLocalSingleAsync(upload => upload.UploadId == envelope.Message.UploadId, ct);
            record.Status = ExtractUploadStatus.Accepted;
            record.TicketId = envelope.Message.TicketId ?? Guid.Empty;
        });

        When<Envelope<RoadNetworkExtractChangesArchiveRejected>>(async (context, envelope, ct) =>
        {
            var record = await context.ExtractUploads.IncludeLocalSingleAsync(upload => upload.UploadId == envelope.Message.UploadId, ct);
            record.Status = ExtractUploadStatus.AutomaticValidationFailed;
        });

        When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, ct) =>
        {
            var download = await context.ExtractDownloads.IncludeLocalSingleOrDefaultAsync(upload => upload.DownloadId == envelope.Message.DownloadId, ct);
            if (download?.LatestUploadId is not null)
            {
                var upload = await context.ExtractUploads.IncludeLocalSingleOrDefaultAsync(upload => upload.UploadId == download.LatestUploadId, ct);
                if (upload is not null)
                {
                    upload.Status = ExtractUploadStatus.Accepted;
                }
            }
        });

        When<Envelope<RoadNetworkChangesRejected>>(async (context, envelope, ct) =>
        {
            ExtractUpload upload = null;

            if (envelope.Message.TicketId is not null)
            {
                upload = await context.ExtractUploads.IncludeLocalSingleOrDefaultAsync(x => x.TicketId == envelope.Message.TicketId, ct);
            }
            else
            {
                // grb uploads
                var extractRequest = await context.ExtractRequests.IncludeLocalSingleOrDefaultAsync(request => request.ExternalRequestId == envelope.Message.Reason, ct);
                if (extractRequest is not null)
                {
                    var download = await context.ExtractDownloads.IncludeLocalSingleOrDefaultAsync(download => download.DownloadId == extractRequest.CurrentDownloadId, ct);
                    if (download?.LatestUploadId is not null)
                    {
                        upload = await context.ExtractUploads.IncludeLocalSingleOrDefaultAsync(x => x.UploadId == download.LatestUploadId, ct);
                    }
                }
            }

            if (upload is not null)
            {
                upload.Status = ExtractUploadStatus.AutomaticValidationFailed;
            }
        });

        When<Envelope<NoRoadNetworkChanges>>(async (context, envelope, ct) =>
        {
            var download = await context.ExtractDownloads.IncludeLocalSingleOrDefaultAsync(download => download.DownloadId == envelope.Message.DownloadId, ct);
            if (download?.LatestUploadId is not null)
            {
                var upload = await context.ExtractUploads.IncludeLocalSingleOrDefaultAsync(x => x.UploadId == download.LatestUploadId, ct);
                if (upload is not null)
                {
                    upload.Status = ExtractUploadStatus.Accepted;
                }
            }
        });
    }
}
