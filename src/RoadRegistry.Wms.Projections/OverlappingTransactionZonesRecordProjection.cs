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
using Polygon = NetTopologySuite.Geometries.Polygon;

public class OverlappingTransactionZonesProjection : ConnectedProjection<WmsContext>
{
    public OverlappingTransactionZonesProjection()
    {
        When<Envelope<RoadNetworkExtractGotRequested>>(async (context, envelope, ct) =>
        {
            var message = envelope.Message;
            if (message.IsInformative)
            {
                return;
            }

            await CreateOverlappingRecords(context, (Geometry)GeometryTranslator.Translate(message.Contour), message.DownloadId, message.Description, ct);
        });

        When<Envelope<RoadNetworkExtractGotRequestedV2>>(async (context, envelope, ct) =>
        {
            var message = envelope.Message;
            if (message.IsInformative)
            {
                return;
            }

            await CreateOverlappingRecords(context, (Geometry)GeometryTranslator.Translate(message.Contour), message.DownloadId, message.Description, ct);
        });

        When<Envelope<RoadNetworkExtractClosed>>(async (context, envelope, ct) =>
        {
            var downloadIds = envelope.Message.DownloadIds.Select(DownloadId.Parse).Select(x => x.ToGuid()).ToArray();
            await DeleteLinkedRecords(context, downloadIds, ct);
        });

        When<Envelope<NoRoadNetworkChanges>>(async (context, envelope, ct) =>
        {
            if (envelope.Message.DownloadId is null)
            {
                return;
            }

            await DeleteLinkedRecords(context, [envelope.Message.DownloadId.Value], ct);
        });

        When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, ct) =>
        {
            if (envelope.Message.DownloadId is null)
            {
                return;
            }

            await DeleteLinkedRecords(context, [envelope.Message.DownloadId.Value], ct);
        });
    }

    private async Task CreateOverlappingRecords(WmsContext context,
        Geometry geometry,
        Guid downloadId,
        string description,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(description))
        {
            description = "onbekend";
        }

        var geometryBoundingBox = geometry.Envelope;

        var transactionZones = await context.TransactionZones
            .IncludeLocalToListAsync(q => q.Where(x => x.Contour.Intersects(geometryBoundingBox)), cancellationToken);

        var overlappingTransactionZones = transactionZones
            .Select(x => new OverlappingTransactionZonesRecord
            {
                DownloadId1 = x.DownloadId,
                DownloadId2 = downloadId,
                Description1 = x.Description,
                Description2 = description,
                Contour = x.Contour.Intersection(geometry) //TODO-rik apply makevalid op beide geometries
            })
            .Where(x => x.Contour is Polygon or MultiPolygon)
            .DistinctBy(x => new { x.DownloadId1, x.DownloadId2 })
            .ToList();
        context.OverlappingTransactionZones.AddRange(overlappingTransactionZones);
    }

    private async Task DeleteLinkedRecords(WmsContext context, Guid[] downloadIds, CancellationToken cancellationToken)
    {
        var recordsToRemove = await context.OverlappingTransactionZones
            .IncludeLocalToListAsync(q =>
                q.Where(x => downloadIds.Contains(x.DownloadId1) || downloadIds.Contains(x.DownloadId2)), cancellationToken);

        context.OverlappingTransactionZones.RemoveRange(recordsToRemove);
    }
}
