namespace RoadRegistry.Wms.Projections;

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

public class ExtractRequestRecordProjection : ConnectedProjection<WmsContext>
{
    public ExtractRequestRecordProjection()
    {
        When<Envelope<RoadNetworkExtractGotRequested>>(async (context, envelope, ct) =>
        {
            var message = envelope.Message;

            if (message.IsInformative)
            {
                return;
            }

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

            if (message.IsInformative)
            {
                return;
            }

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

        When<Envelope<RoadNetworkExtractDownloaded>>(async (context, envelope, ct) =>
        {
            if (envelope.Message.IsInformative)
            {
                return;
            }

            var record = await context.ExtractRequests
                .IncludeLocalSingleOrDefaultAsync(record => record.DownloadId == envelope.Message.DownloadId, ct);
            if (record is null)
            {
                return;
            }

            record.DownloadedOn = InstantPattern.ExtendedIso.Parse(envelope.Message.When).Value.ToDateTimeOffset();
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

    private async Task CloseExtractRequests(
        WmsContext editorContext,
        Guid[] downloadIds,
        CancellationToken cancellationToken)
    {
        var records = await editorContext.ExtractRequests
            .IncludeLocalToListAsync(q => q
                .Where(record => downloadIds.Contains(record.DownloadId)), cancellationToken);

        editorContext.ExtractRequests.RemoveRange(records);
    }
}
