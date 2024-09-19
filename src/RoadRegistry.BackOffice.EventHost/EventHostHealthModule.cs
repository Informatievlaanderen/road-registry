namespace RoadRegistry.BackOffice.EventHost;

using System;
using Autofac;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;
using TicketingService.Abstractions;
using Uploads;

public class EventHostHealthModule : EventHandlerModule
{
    public EventHostHealthModule(
        ILifetimeScope lifetimeScope,
        RoadNetworkUploadsBlobClient uploadsBlobClient,
        ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(lifetimeScope);
        ArgumentNullException.ThrowIfNull(uploadsBlobClient);

        var logger = loggerFactory.ThrowIfNull().CreateLogger(GetType());

        For<EventHostSystemHealthCheckRequested>()
            .Handle(async (message, ct) =>
            {
                var ticketId = new TicketId(message.Body.TicketId);

                await using var container = lifetimeScope.BeginLifetimeScope();
                var ticketing = container.Resolve<ITicketing>();
                try
                {
                    var bucketFileName = new BlobName(message.Body.BucketFileName);

                    await uploadsBlobClient.GetBlobAsync(bucketFileName, ct);

                    await ticketing.Complete(ticketId, new TicketResult(), ct);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"{nameof(EventHostSystemHealthCheckRequested)} failed");

                    await ticketing.Error(ticketId, new TicketError(), ct);
                }
            });
    }
}
