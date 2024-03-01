namespace RoadRegistry.Snapshot.Handlers;

using Autofac;
using BackOffice;
using BackOffice.Abstractions.RoadNetworks;
using BackOffice.Core;
using BackOffice.Framework;
using BackOffice.Messages;
using MediatR;
using Microsoft.Extensions.Logging;
using NodaTime;
using SqlStreamStore;
using Sqs.RoadNetworks;
using System;

public class RoadNetworkSnapshotEventModule : EventHandlerModule
{
    public RoadNetworkSnapshotEventModule(
        IStreamStore store,
        ILifetimeScope lifetimeScope,
        IMediator mediator,
        IRoadNetworkSnapshotReader snapshotReader,
        IRoadNetworkSnapshotWriter snapshotWriter,
        IClock clock,
        ILoggerFactory loggerFactory
    )
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(snapshotReader);
        ArgumentNullException.ThrowIfNull(snapshotWriter);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        var logger = loggerFactory.CreateLogger<RoadNetworkSnapshotEventModule>();
        var enricher = EnrichEvent.WithTime(clock);

        For<RoadNetworkChangesAccepted>()
            .UseRoadRegistryContext(store, lifetimeScope, snapshotReader, loggerFactory, enricher)
            .Handle(async (context, message, ct) =>
            {
                logger.LogInformation("Event handler started for {EventName}", nameof(RoadNetworkChangesAccepted));

                await mediator.Send(new CreateRoadNetworkSnapshotSqsRequest
                {
                    ProvenanceData = new RoadRegistryProvenanceData(),
                    Metadata = new Dictionary<string, object?>
                    {
                        { "CorrelationId", message.MessageId }
                    },
                    Request = new CreateRoadNetworkSnapshotRequest { StreamVersion = message.StreamVersion }
                }, ct);

                logger.LogInformation("Event handler finished for {EventName}", nameof(RoadNetworkChangesAccepted));
            });
    }
}
