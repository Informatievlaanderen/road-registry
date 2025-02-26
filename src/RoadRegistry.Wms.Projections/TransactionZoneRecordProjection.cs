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
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Operation.Overlay;
using NetTopologySuite.Operation.OverlayNG;
using Schema;
using Polygon = NetTopologySuite.Geometries.Polygon;

public class TransactionZoneRecordProjection : ConnectedProjection<WmsContext>
{
    private readonly ILogger _logger;

    public TransactionZoneRecordProjection(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger(GetType());

        When<Envelope<RoadNetworkExtractGotRequested>>(async (context, envelope, ct) =>
        {
            var message = envelope.Message;

            if (message.IsInformative)
            {
                return;
            }

            var contour = (Geometry)GeometryTranslator.Translate(message.Contour);

            var record = new TransactionZoneRecord
            {
                Contour = contour,
                DownloadId = message.DownloadId,
                Description = !string.IsNullOrEmpty(message.Description) ? message.Description : "onbekend"
            };
            context.TransactionZones.Add(record);

            await CreateOverlappingRecords(context,
                contour,
                message.DownloadId,
                message.Description,
                ct);
        });

        When<Envelope<RoadNetworkExtractGotRequestedV2>>(async (context, envelope, ct) =>
        {
            var message = envelope.Message;

            if (message.IsInformative)
            {
                return;
            }

            var contour = (Geometry)GeometryTranslator.Translate(message.Contour);

            var record = new TransactionZoneRecord
            {
                Contour = contour,
                DownloadId = message.DownloadId,
                Description = !string.IsNullOrEmpty(message.Description) ? message.Description : "onbekend"
            };
            context.TransactionZones.Add(record);

            await CreateOverlappingRecords(context,
                contour,
                message.DownloadId,
                message.Description,
                ct);
        });

        When<Envelope<RoadNetworkExtractDownloaded>>((_, _, _) => Task.CompletedTask);

        When<Envelope<RoadNetworkExtractClosed>>(async (context, envelope, ct) =>
        {
            var downloadIds = envelope.Message.DownloadIds
                .Select(DownloadId.Parse)
                .Select(x => x.ToGuid())
                .ToArray();
            await RemoveTransactionZones(context, downloadIds, ct);
        });

        When<Envelope<NoRoadNetworkChanges>>((_, _, _) => Task.CompletedTask);

        When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, ct) =>
        {
            if (envelope.Message.DownloadId is null)
            {
                return;
            }

            await RemoveTransactionZones(context, [envelope.Message.DownloadId.Value], ct);
        });
    }

    private async Task CreateOverlappingRecords(
        WmsContext context,
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
            .IncludeLocalToListAsync(q =>
                    q.Where(x => x.DownloadId != downloadId && x.Contour.Intersects(geometryBoundingBox)),
                cancellationToken);

        var overlappingTransactionZones = transactionZones
            .Select(x =>
            {
                try
                {
                    return new OverlappingTransactionZonesRecord
                    {
                        DownloadId1 = x.DownloadId,
                        DownloadId2 = downloadId,
                        Description1 = x.Description,
                        Description2 = description,
                        Contour = OverlayNGRobust.Overlay(x.Contour, geometry, SpatialFunction.Intersection)
                    };
                }
                catch (TopologyException)
                {
                    _logger.LogError("An error occurred during overlay calculation between geometries [{Wkt1}] and [{Wkt2}]", x.Contour.AsText(), geometry.AsText());
                    throw;
                }
            })
            .Where(x =>
                (x.Contour is Polygon polygon && polygon.Area.ToRoundedMeasurement() > 0)
                ||(x.Contour is MultiPolygon multiPolygon && multiPolygon.Area.ToRoundedMeasurement() > 0))
            .DistinctBy(x => new { x.DownloadId1, x.DownloadId2 })
            .ToList();

        context.OverlappingTransactionZones.AddRange(overlappingTransactionZones);
    }

    private async Task RemoveTransactionZones(
        WmsContext wmsContext,
        Guid[] downloadIds,
        CancellationToken cancellationToken)
    {
        var transactionZones = await wmsContext.TransactionZones
            .IncludeLocalToListAsync(q => q
                .Where(record => downloadIds.Contains(record.DownloadId)),
                cancellationToken);
        wmsContext.TransactionZones.RemoveRange(transactionZones);

        var overlappingTransactionZones = await wmsContext.OverlappingTransactionZones
            .IncludeLocalToListAsync(q =>
                q.Where(x =>
                    downloadIds.Contains(x.DownloadId1) || downloadIds.Contains(x.DownloadId2)),
                cancellationToken);
        wmsContext.OverlappingTransactionZones.RemoveRange(overlappingTransactionZones);
    }
}
