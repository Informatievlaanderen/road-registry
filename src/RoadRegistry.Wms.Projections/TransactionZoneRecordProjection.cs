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
using Schema;

public class TransactionZoneRecordProjection : ConnectedProjection<WmsContext>
{
    public TransactionZoneRecordProjection()
    {
        When<Envelope<RoadNetworkExtractGotRequested>>(async (context, envelope, ct) =>
        {
            var message = envelope.Message;

            if (message.IsInformative)
            {
                return;
            }

            var record = new TransactionZoneRecord
            {
                Contour = (Geometry)GeometryTranslator.Translate(message.Contour),
                DownloadId = message.DownloadId,
                Description = !string.IsNullOrEmpty(message.Description) ? message.Description : "onbekend"
            };
            context.TransactionZones.Add(record);
        });

        When<Envelope<RoadNetworkExtractGotRequestedV2>>(async (context, envelope, ct) =>
        {
            var message = envelope.Message;

            if (message.IsInformative)
            {
                return;
            }

            var record = new TransactionZoneRecord
            {
                Contour = (Geometry)GeometryTranslator.Translate(message.Contour),
                DownloadId = message.DownloadId,
                Description = !string.IsNullOrEmpty(message.Description) ? message.Description : "onbekend"
            };
            context.TransactionZones.Add(record);
        });

        When<Envelope<RoadNetworkExtractDownloaded>>((_, _, _) => Task.CompletedTask);

        When<Envelope<RoadNetworkExtractClosed>>(async (context, envelope, ct) =>
        {
            await RemoveExtractRequests(context, envelope.Message.DownloadIds.Select(x => DownloadId.Parse(x).ToGuid()).ToArray(), ct);
        });

        When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, ct) =>
        {
            if (envelope.Message.DownloadId is null)
            {
                return;
            }

            await RemoveExtractRequests(context, [envelope.Message.DownloadId.Value], ct);
        });
    }

    private async Task RemoveExtractRequests(
        WmsContext editorContext,
        Guid[] downloadIds,
        CancellationToken cancellationToken)
    {
        var records = await editorContext.TransactionZones
            .IncludeLocalToListAsync(q => q
                .Where(record => downloadIds.Contains(record.DownloadId)), cancellationToken);

        editorContext.TransactionZones.RemoveRange(records);
    }
}
