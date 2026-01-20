namespace RoadRegistry.Extracts.Projections;

using System;
using System.Linq;
using BackOffice.Extensions;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using NodaTime.Text;
using Schema;

public class ExtractRequestProjection : ConnectedProjection<ExtractsDbContext>
{
    public ExtractRequestProjection()
    {
        When<Envelope<RoadNetworkExtractGotRequested>>(async (context, envelope, ct) =>
        {
            var message = envelope.Message;
            var extractRequestId = ExtractRequestId.FromExternalRequestId(new ExternalExtractRequestId(message.ExternalRequestId));

            var record = await context.ExtractRequests.IncludeLocalSingleOrDefaultAsync(x => x.ExtractRequestId == extractRequestId, ct);
            if (record is null)
            {
                record = new ExtractRequest
                {
                    RequestedOn = DateTime.Parse(message.When),
                    ExtractRequestId = ExtractRequestId.FromExternalRequestId(new ExternalExtractRequestId(message.ExternalRequestId)),
                    ExternalRequestId = message.ExternalRequestId,
                    Description = !string.IsNullOrEmpty(message.Description) ? message.Description : "onbekend",
                    OrganizationCode = "",
                    CurrentDownloadId = message.DownloadId,
                };
                await context.ExtractRequests.AddAsync(record, ct);
            }
        });

        When<Envelope<RoadNetworkExtractGotRequestedV2>>(async (context, envelope, ct) =>
        {
            var message = envelope.Message;

            var organizationCode = "";
            if (envelope.Metadata.TryGetValue("provenanceData", out var provenanceData))
            {
                organizationCode = ((JObject)provenanceData)["operator"]?.Value<string>() ?? "";
            }

            var extractRequestId = ExtractRequestId.FromExternalRequestId(new ExternalExtractRequestId(envelope.Message.ExternalRequestId));

            var record = await context.ExtractRequests.IncludeLocalSingleOrDefaultAsync(x => x.ExtractRequestId == extractRequestId, ct);
            if (record is null)
            {
                record = new ExtractRequest
                {
                    RequestedOn = DateTime.Parse(message.When),
                    ExtractRequestId = ExtractRequestId.FromExternalRequestId(new ExternalExtractRequestId(message.ExternalRequestId)),
                    ExternalRequestId = message.ExternalRequestId,
                    Description = !string.IsNullOrEmpty(message.Description) ? message.Description : "onbekend",
                    OrganizationCode = organizationCode,
                    CurrentDownloadId = message.DownloadId,
                };
                await context.ExtractRequests.AddAsync(record, ct);
            }
            else if (record.CurrentDownloadId != message.DownloadId)
            {
                var requestedOn = InstantPattern.ExtendedIso.Parse(envelope.Message.When).Value.ToDateTimeOffset();
                var existingOpenDownloads = await context.ExtractDownloads
                    .Where(x => x.ExtractRequestId == extractRequestId && x.Closed == false && x.DownloadId != message.DownloadId && x.RequestedOn < requestedOn)
                    .ToListAsync(ct);
                foreach(var existingOpenDownload in existingOpenDownloads)
                {
                    existingOpenDownload.Closed = true;
                }

                record.CurrentDownloadId = message.DownloadId;
            }
        });
    }
}
