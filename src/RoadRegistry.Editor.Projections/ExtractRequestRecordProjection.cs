namespace RoadRegistry.Editor.Projections;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackOffice;
using BackOffice.Extensions;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using NetTopologySuite.Geometries;
using NodaTime.Text;
using Schema;
using Schema.Extracts;

public class ExtractRequestRecordProjection : ConnectedProjection<EditorContext>
{
    public ExtractRequestRecordProjection()
    {
        When<Envelope<RoadNetworkExtractGotRequested>>(async (context, envelope, ct) =>
        {
            var message = envelope.Message;
            var record = new ExtractRequestRecord
            {
                RequestedOn = DateTime.Parse(message.When),
                ExternalRequestId = message.ExternalRequestId,
                Contour = (Geometry)GeometryTranslator.Translate(message.Contour),
                DownloadId = message.DownloadId,
                Description = !string.IsNullOrEmpty(message.Description) ? message.Description : "onbekend",
                IsInformative = message.IsInformative
            };
            await context.ExtractRequests.AddAsync(record, ct);
        });

        When<Envelope<RoadNetworkExtractGotRequestedV2>>(async (context, envelope, ct) =>
        {
            var message = envelope.Message;
            var record = new ExtractRequestRecord
            {
                RequestedOn = DateTime.Parse(message.When),
                ExternalRequestId = message.ExternalRequestId,
                Contour = (Geometry)GeometryTranslator.Translate(message.Contour),
                DownloadId = message.DownloadId,
                Description = !string.IsNullOrEmpty(message.Description) ? message.Description : "onbekend",
                IsInformative = message.IsInformative
            };
            await context.ExtractRequests.AddAsync(record, ct);
        });

        When<Envelope<RoadNetworkExtractDownloadBecameAvailable>>(async (context, envelope, ct) =>
        {
            await UpdateExtractRequest(context, envelope.Message.DownloadId, record =>
            {
                record.DownloadAvailable = true;
            }, ct);
        });

        When<Envelope<RoadNetworkExtractDownloadTimeoutOccurred>>(async (context, envelope, ct) =>
        {
            if (envelope.Message.DownloadId is null)
            {
                return;
            }

            await UpdateExtractRequest(context, envelope.Message.DownloadId.Value, record =>
            {
                record.ExtractDownloadTimeoutOccurred = true;
            }, ct);
        });

        When<Envelope<RoadNetworkExtractDownloaded>>(async (context, envelope, ct) =>
        {
            await UpdateExtractRequest(context, envelope.Message.DownloadId, record =>
            {
                record.DownloadedOn = InstantPattern.ExtendedIso.Parse(envelope.Message.When).Value.ToDateTimeOffset();
            }, ct);
        });

        When<Envelope<RoadNetworkChangesArchiveUploaded>>(async (context, envelope, ct) =>
        {
            if (envelope.Message.DownloadId is null)
            {
                return;
            }

            await UpdateExtractRequest(context, envelope.Message.DownloadId.Value, record =>
            {
                record.ArchiveId = envelope.Message.ArchiveId;
                record.TicketId = envelope.Message.TicketId;
            }, ct);
        });

        When<Envelope<RoadNetworkExtractChangesArchiveUploaded>>(async (context, envelope, ct) =>
        {
            await UpdateExtractRequest(context, envelope.Message.DownloadId, record =>
            {
                record.ArchiveId = envelope.Message.ArchiveId;
                record.TicketId = envelope.Message.TicketId;
            }, ct);
        });

        When<Envelope<RoadNetworkExtractClosed>>(async (context, envelope, ct) =>
        {
            await CloseExtractRequests(context, envelope.Message.DownloadIds.Select(x => DownloadId.Parse(x).ToGuid()).ToArray(), ct);
        });

        When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, ct) =>
        {
            if (envelope.Message.DownloadId is null)
            {
                return;
            }

            await CloseExtractRequests(context, [envelope.Message.DownloadId.Value], ct);
        });
    }

    private async Task UpdateExtractRequest(EditorContext editorContext, Guid downloadId, Action<ExtractRequestRecord> updateAction, CancellationToken cancellationToken)
    {
        var record = await editorContext.ExtractRequests
            .IncludeLocalSingleOrDefaultAsync(r => r.DownloadId == downloadId, cancellationToken);
        if (record is not null)
        {
            updateAction(record);
        }
    }

    private async Task CloseExtractRequests(EditorContext editorContext, Guid[] downloadIds, CancellationToken cancellationToken)
    {
        var records = await editorContext.ExtractRequests
            .IncludeLocalToListAsync(q => q
                .Where(record => downloadIds.Contains(record.DownloadId)), cancellationToken);

        records.ForEach(record => record.IsInformative = true);
    }
}
