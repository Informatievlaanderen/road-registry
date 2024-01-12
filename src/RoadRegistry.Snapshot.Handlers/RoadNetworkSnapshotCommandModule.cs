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
using Autofac;
using SqlStreamStore.Streams;

public class RoadNetworkSnapshotCommandModule : CommandHandlerModule
{
    public RoadNetworkSnapshotCommandModule(
        IStreamStore store,
        IMediator mediator,
        ILifetimeScope lifetimeScope,
        IRoadNetworkSnapshotReader snapshotReader,
        IRoadNetworkSnapshotWriter snapshotWriter,
        IClock clock,
        IRoadNetworkEventWriter roadNetworkEventWriter,
        ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(snapshotReader);
        ArgumentNullException.ThrowIfNull(snapshotWriter);
        ArgumentNullException.ThrowIfNull(roadNetworkEventWriter);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        var logger = loggerFactory.CreateLogger<RoadNetworkSnapshotEventModule>();
        var enricher = EnrichEvent.WithTime(clock);

        For<RebuildRoadNetworkSnapshot>()
            .UseRoadRegistryContext(store, lifetimeScope, snapshotReader, loggerFactory, enricher)
            .Handle(async (context, command, applicationMetadata, ct) =>
            {
                logger.LogInformation("Command handler started for {CommandName}", nameof(RebuildRoadNetworkSnapshot));

                var (network, version) = await context.RoadNetworks.GetWithVersion(false, null, ct);
                await snapshotWriter.WriteSnapshot(network.TakeSnapshot(), version, ct);

                var completedEvent = new RebuildRoadNetworkSnapshotCompleted
                {
                    CurrentVersion = version
                };
                //TODO-rik test
                await roadNetworkEventWriter.WriteAsync(RoadNetworkStreamNameProvider.Default, command, ExpectedVersion.Any, new object[] { completedEvent }, ct);

                logger.LogInformation("Command handler finished for {Command}", nameof(RebuildRoadNetworkSnapshot));
            });
    }
}
