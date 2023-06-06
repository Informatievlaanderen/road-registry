namespace RoadRegistry.Editor.Projections;

using BackOffice;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using NodaTime.Text;
using Schema;
using Schema.Extracts;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

        When<Envelope<RoadNetworkExtractDownloaded>>(async (context, envelope, ct) =>
        {
            if (envelope.Message.DownloadId == Guid.Empty)
            {
                return;
            }

            var record = context.ExtractRequests.Local.SingleOrDefault(record => record.DownloadId == envelope.Message.DownloadId)
                                 ?? await context.ExtractRequests.SingleAsync(record => record.DownloadId == envelope.Message.DownloadId, ct);

            record.DownloadedOn = InstantPattern.ExtendedIso.Parse(envelope.Message.When).Value.ToDateTimeOffset();
        });

        When<Envelope<RoadNetworkExtractClosed>>(async (context, envelope, ct) =>
        {
            await CloseExtractRequests(context, envelope.Message.ExternalRequestId, ct);
        });

        When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, ct) =>
        {
            if (envelope.Message.DownloadId is null)
            {
                return;
            }
            
            var downloadRecord = context.ExtractRequests.Local.SingleOrDefault(record => record.DownloadId == envelope.Message.DownloadId.Value)
                ?? await context.ExtractRequests.SingleAsync(record => record.DownloadId == envelope.Message.DownloadId.Value, ct);

            await CloseExtractRequests(context, downloadRecord.ExternalRequestId, ct);
        });
    }

    private async Task CloseExtractRequests(EditorContext editorContext, string externalRequestId, CancellationToken cancellationToken)
    {
        var records = editorContext.ExtractRequests.Local
            .Where(record => record.ExternalRequestId == externalRequestId)
            .ToList()
            .Concat(await editorContext.ExtractRequests
                .Where(record => record.ExternalRequestId == externalRequestId)
                .ToListAsync(cancellationToken))
            ;

        foreach (var record in records)
        {
            record.IsInformative = true;
        }
    }
}
