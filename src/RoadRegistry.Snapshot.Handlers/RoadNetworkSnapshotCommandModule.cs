namespace RoadRegistry.Snapshot.Handlers;

using MediatR;
using Microsoft.Extensions.Logging;
using NodaTime;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.Messages;
using SqlStreamStore;
using System;

public class RoadNetworkSnapshotCommandModule : CommandHandlerModule
{
    public RoadNetworkSnapshotCommandModule(
        IStreamStore store,
        IMediator mediator,
        Func<EventSourcedEntityMap> entityMapFactory,
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
        var enricher = EnrichEvent.WithTime(clock);

        For<RebuildRoadNetworkSnapshot>()
            .UseRoadRegistryContext(store, entityMapFactory, snapshotReader, loggerFactory, enricher)
            .Handle(async (context, command, applicationMetadata, ct) =>
            {
                logger.LogInformation("Command handler started for {CommandName}", nameof(RebuildRoadNetworkSnapshot));

                var (network, version) = await context.RoadNetworks.GetWithVersion(false, null, ct);
                await snapshotWriter.WriteSnapshot(network.TakeSnapshot(), version, ct);

                var completedCommand = new RebuildRoadNetworkSnapshotCompleted
                {
                    CurrentVersion = version
                };

                await new RoadNetworkCommandQueue(store, applicationMetadata)
                    .Write(new Command(completedCommand), ct);

                logger.LogInformation("Command handler finished for {Command}", nameof(RebuildRoadNetworkSnapshot));
            });

        For<RebuildRoadNetworkSnapshotCompleted>()
            .Handle((_, _, _) =>
            {
                logger.LogInformation("Command handler started for {CommandName}", nameof(RebuildRoadNetworkSnapshotCompleted));
                logger.LogInformation("Command handler finished for {Command}", nameof(RebuildRoadNetworkSnapshotCompleted));
                return Task.CompletedTask;
            });
    }
}
