namespace RoadRegistry.BackOffice.Handlers;

using Core;
using MediatR;
using Microsoft.Extensions.Logging;
using NodaTime;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.Messages;
using SqlStreamStore;
using System;

public class RoadNetworkBackOfficeEventModule : EventHandlerModule
{
    public RoadNetworkBackOfficeEventModule(
        IStreamStore store,
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

        var logger = loggerFactory.CreateLogger<RoadNetworkBackOfficeEventModule>();

        For<CompletedRoadNetworkImport>()
            .UseRoadRegistryContext(store, snapshotReader, loggerFactory, EnrichEvent.WithTime(clock))
            .Handle(async (context, message, ct) =>
            {
                logger.LogInformation("Event handler started for {EventName}", nameof(RoadNetworkChangesAccepted));

                var (network, version) = await context.RoadNetworks.GetWithVersion(ct);
                await snapshotWriter.WriteSnapshot(network.TakeSnapshot(), version, ct);

                logger.LogInformation("Event handler finished for {EventName}", nameof(CompletedRoadNetworkImport));
            });
    }
}
