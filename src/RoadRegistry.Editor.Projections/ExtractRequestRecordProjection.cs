namespace RoadRegistry.Editor.Projections;

using BackOffice;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Schema;
using Schema.Extracts;
using System;
using System.Linq;

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
                Description = message.Description,
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
                Description = message.Description,
                IsInformative = message.IsInformative
            };
            await context.ExtractRequests.AddAsync(record, ct);
        });

        When<Envelope<RoadNetworkExtractClosed>>(async (context, envelope, ct) =>
        {
            var record = context.ExtractRequests.Local.SingleOrDefault(record => record.ExternalRequestId == envelope.Message.ExternalRequestId)
                ?? await context.ExtractRequests.SingleAsync(record => record.ExternalRequestId == envelope.Message.ExternalRequestId, ct);
            record.IsInformative = true;
        });
    }
}
