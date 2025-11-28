namespace RoadRegistry.BackOffice.ExtractHost;

using System;
using Autofac;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Extracts;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;
using RoadRegistry.Extensions;
using TicketingService.Abstractions;

public class ExtractHostHealthModule : EventHandlerModule
{
    public ExtractHostHealthModule(
        ILifetimeScope lifetimeScope,
        RoadNetworkExtractDownloadsBlobClient downloadsBlobClient,
        RoadNetworkExtractUploadsBlobClient uploadsBlobClient,
        ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(lifetimeScope);
        ArgumentNullException.ThrowIfNull(downloadsBlobClient);
        ArgumentNullException.ThrowIfNull(uploadsBlobClient);

        var logger = loggerFactory.ThrowIfNull().CreateLogger(GetType());

        For<ExtractHostSystemHealthCheckRequested>()
            .Handle(async (message, ct) =>
            {
                var ticketId = new TicketId(message.Body.TicketId);

                await using var container = lifetimeScope.BeginLifetimeScope();
                var ticketing = container.Resolve<ITicketing>();
                try
                {
                    var bucketFileName = new BlobName(message.Body.BucketFileName);

                    await downloadsBlobClient.GetBlobAsync(bucketFileName, ct);
                    await downloadsBlobClient.DeleteBlobAsync(bucketFileName, ct);

                    await uploadsBlobClient.GetBlobAsync(bucketFileName, ct);

                    await ticketing.Complete(ticketId, new TicketResult(), ct);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"{nameof(ExtractHostSystemHealthCheckRequested)} failed");

                    await ticketing.Error(ticketId, new TicketError(), ct);
                }
            });
    }
}
