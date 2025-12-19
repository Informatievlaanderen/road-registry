namespace RoadRegistry.Editor.Projections;

using System.Linq;
using BackOffice;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Microsoft.EntityFrameworkCore;
using NodaTime.Text;
using Schema;
using Schema.Extracts;

public class ExtractUploadRecordProjection : ConnectedProjection<EditorContext>
{
    public ExtractUploadRecordProjection()
    {
        When<Envelope<RoadNetworkExtractChangesArchiveUploaded>>(async (context, envelope, ct) =>
        {
            var record = new ExtractUploadRecord
            {
                UploadId = envelope.Message.UploadId,
                ExternalRequestId = envelope.Message.ExternalRequestId,
                RequestId = envelope.Message.RequestId,
                DownloadId = envelope.Message.DownloadId,
                ArchiveId = envelope.Message.ArchiveId,
                ChangeRequestId = ChangeRequestId.FromUploadId(new UploadId(envelope.Message.UploadId)),
                ReceivedOn = InstantPattern.ExtendedIso.Parse(envelope.Message.When).Value.ToUnixTimeSeconds(),
                Status = ExtractUploadStatus.Received,
                CompletedOn = 0L
            };
            await context.ExtractUploads.AddAsync(record, ct);
        });

        When<Envelope<RoadNetworkExtractChangesArchiveAccepted>>(async (context, envelope, ct) =>
        {
            var record =
                context.ExtractUploads.Local.SingleOrDefault(upload => upload.UploadId == envelope.Message.UploadId)
                ?? await context.ExtractUploads.SingleAsync(upload => upload.UploadId == envelope.Message.UploadId, ct);
            record.Status = ExtractUploadStatus.UploadAccepted;
        });

        When<Envelope<RoadNetworkExtractChangesArchiveRejected>>(async (context, envelope, ct) =>
        {
            var record =
                context.ExtractUploads.Local.SingleOrDefault(upload => upload.UploadId == envelope.Message.UploadId)
                ?? await context.ExtractUploads.SingleAsync(upload => upload.UploadId == envelope.Message.UploadId, ct);
            record.Status = ExtractUploadStatus.UploadRejected;
            record.CompletedOn = InstantPattern.ExtendedIso.Parse(envelope.Message.When).Value.ToUnixTimeSeconds();
        });

        When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, ct) =>
        {
            // NOTE: Not all changes to the road network are caused by extract uploads.
            // Hence why we need to treat this conditionally.
            var record =
                context.ExtractUploads.Local.SingleOrDefault(upload => upload.ChangeRequestId == envelope.Message.RequestId)
                ?? await context.ExtractUploads.SingleOrDefaultAsync(upload => upload.ChangeRequestId == envelope.Message.RequestId, ct);
            if (record != null)
            {
                record.Status = ExtractUploadStatus.ChangesAccepted;
                record.CompletedOn = InstantPattern.ExtendedIso.Parse(envelope.Message.When).Value
                    .ToUnixTimeSeconds();
            }
        });

        When<Envelope<RoadNetworkChangesRejected>>(async (context, envelope, ct) =>
        {
            // NOTE: Not all changes to the road network are caused by extract uploads.
            // Hence why we need to treat this conditionally.
            var record =
                context.ExtractUploads.Local.SingleOrDefault(upload => upload.ChangeRequestId == envelope.Message.RequestId)
                ?? await context.ExtractUploads.SingleOrDefaultAsync(upload => upload.ChangeRequestId == envelope.Message.RequestId, ct);
            if (record != null)
            {
                record.Status = ExtractUploadStatus.ChangesRejected;
                record.CompletedOn = InstantPattern.ExtendedIso.Parse(envelope.Message.When).Value
                    .ToUnixTimeSeconds();
            }
        });

        When<Envelope<NoRoadNetworkChanges>>(async (context, envelope, ct) =>
        {
            // NOTE: Not all changes to the road network are caused by extract uploads.
            // Hence why we need to treat this conditionally.
            var record =
                context.ExtractUploads.Local.SingleOrDefault(upload => upload.ChangeRequestId == envelope.Message.RequestId)
                ?? await context.ExtractUploads.SingleOrDefaultAsync(upload => upload.ChangeRequestId == envelope.Message.RequestId, ct);
            if (record != null)
            {
                record.Status = ExtractUploadStatus.NoChanges;
                record.CompletedOn = InstantPattern.ExtendedIso.Parse(envelope.Message.When).Value
                    .ToUnixTimeSeconds();
            }
        });
    }
}
