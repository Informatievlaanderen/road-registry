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
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Schema;

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

    private async Task CreateOverlappingRecords(WmsContext context, Geometry geometry, Guid downloadId, string description, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(description))
        {
            description = "onbekend";
        }

        var overlapRecords = await context.OverlappingTransactionZones.FromSqlRaw(@"
SELECT o.*
FROM (
SELECT
    0 As Id
	, r.[DownloadId] DownloadId1
	, @downloadId DownloadId2
	, r.[Description] Description1
	, @description Description2
	, r.[Contour].MakeValid().STIntersection(@contour.MakeValid()) Contour
FROM [RoadRegistryEditor].[ExtractRequest] r
WHERE r.IsInformative = 0
    AND r.[DownloadId] <> @downloadId
	AND r.[Contour].MakeValid().STIntersects(@contour.MakeValid()) = 1
) as o
WHERE o.Contour.STIsEmpty() = 0 AND o.Contour.STGeometryType() LIKE '%POLYGON'
",
                geometry.ToSqlParameter("contour"),
                new SqlParameter("downloadId", downloadId),
                new SqlParameter("description", description))
            .ToListAsync(cancellationToken);

        overlapRecords = overlapRecords
            .DistinctBy(x => new { x.DownloadId1, x.DownloadId2 })
            .ToList();

        await context.OverlappingTransactionZones.AddRangeAsync(overlapRecords, cancellationToken);
    }

    private async Task DeleteLinkedRecords(WmsContext context, Guid[] downloadIds, CancellationToken cancellationToken)
    {
        var requestsToRemoved = await context.OverlappingTransactionZones
            .IncludeLocalToListAsync(q =>
                q.Where(x => downloadIds.Contains(x.DownloadId1) || downloadIds.Contains(x.DownloadId2)), cancellationToken);

        context.OverlappingTransactionZones.RemoveRange(requestsToRemoved);
    }
}
