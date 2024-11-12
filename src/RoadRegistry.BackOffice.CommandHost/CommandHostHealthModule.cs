namespace RoadRegistry.BackOffice.CommandHost;

using System;
using Autofac;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Core;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;
using NodaTime;
using SqlStreamStore;
using TicketingService.Abstractions;
using Uploads;

public class CommandHostHealthModule : CommandHandlerModule
{
    public CommandHostHealthModule(
        IStreamStore store,
        ILifetimeScope lifetimeScope,
        IRoadNetworkSnapshotReader snapshotReader,
        IClock clock,
        ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(lifetimeScope);
        ArgumentNullException.ThrowIfNull(snapshotReader);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        var logger = loggerFactory.CreateLogger(GetType());

        var enricher = EnrichEvent.WithTime(clock);

        For<CheckCommandHostHealth>()
            .UseRoadRegistryContext(store, lifetimeScope, snapshotReader, loggerFactory, enricher)
            .Handle(async (context, command, applicationMetadata, cancellationToken) =>
            {
                var ticketId = new TicketId(command.Body.TicketId);

                await using var container = lifetimeScope.BeginLifetimeScope();
                var ticketing = container.Resolve<ITicketing>();

                try
                {
                    await snapshotReader.ReadSnapshotVersionAsync(cancellationToken);

                    var blobClient = container.Resolve<RoadNetworkUploadsBlobClient>();
                    await blobClient.GetBlobAsync(new BlobName(command.Body.FileName), cancellationToken);

                    await ticketing.Complete(ticketId, new TicketResult(), cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"{nameof(CheckCommandHostHealth)} failed");

                    await ticketing.Error(ticketId, new TicketError(), cancellationToken);
                }
            });
    }
}
