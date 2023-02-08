namespace RoadRegistry.BackOffice.Handlers;

using Core;
using MediatR;
using Microsoft.Extensions.Logging;
using NodaTime;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.Messages;
using RoadRegistry.Snapshot.Handlers.Sqs.RoadNetworks;
using SqlStreamStore;
using System;
using Abstractions.RoadNetworks;

public class RoadNetworkSnapshotEventModule : EventHandlerModule
{
    public RoadNetworkSnapshotEventModule(
        IStreamStore store,
        IMediator mediator,
        IRoadNetworkSnapshotReader snapshotReader,
        IRoadNetworkSnapshotWriter snapshotWriter,
        IClock clock,
        ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(snapshotReader);
        ArgumentNullException.ThrowIfNull(snapshotWriter);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        var logger = loggerFactory.CreateLogger<RoadNetworkSnapshotEventModule>();

        For<RoadNetworkChangesAccepted>()
            .UseRoadRegistryContext(store, snapshotReader, loggerFactory, EnrichEvent.WithTime(clock))
            .Handle(async (context, message, ct) =>
            {
                logger.LogInformation("Event handler started for {EventName}", nameof(RoadNetworkChangesAccepted));

                await mediator.Send(new CreateRoadNetworkSnapshotSqsRequest
                {
                    Request = new CreateRoadNetworkSnapshotRequest
                    {
                        StreamVersion = message.StreamVersion
                    }
                }, ct);

                logger.LogInformation("Event handler finished for {EventName}", nameof(RoadNetworkChangesAccepted));
            });
    }
}
