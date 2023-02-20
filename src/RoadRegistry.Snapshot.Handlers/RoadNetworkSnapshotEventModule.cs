namespace RoadRegistry.Snapshot.Handlers;

using System;
using BackOffice.FeatureToggles;
using MediatR;
using Microsoft.Extensions.Logging;
using NodaTime;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Abstractions.RoadNetworks;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.Messages;
using RoadRegistry.Snapshot.Handlers.Sqs.RoadNetworks;
using SqlStreamStore;

public class RoadNetworkSnapshotEventModule : EventHandlerModule
{
    public RoadNetworkSnapshotEventModule(
        IStreamStore store,
        IMediator mediator,
        IRoadNetworkSnapshotReader snapshotReader,
        IRoadNetworkSnapshotWriter snapshotWriter,
        IClock clock,
        ILoggerFactory loggerFactory,
        UseSnapshotSqsRequestFeatureToggle snapshotFeatureToggle,
        ApplicationMetadata applicationMetadata
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
            .UseRoadRegistryContext(store, snapshotReader, loggerFactory, enricher)
            .Handle(async (context, message, ct) =>
            {
                logger.LogInformation("Event handler started for {EventName}", nameof(RoadNetworkChangesAccepted));

                if (snapshotFeatureToggle.FeatureEnabled)
                {
                    await mediator.Send(new CreateRoadNetworkSnapshotSqsRequest { Request = new CreateRoadNetworkSnapshotRequest { StreamVersion = message.StreamVersion } }, ct);
                }
                else
                {
                    var (network, version) = await context.RoadNetworks.GetWithVersion(ct);
                    await snapshotWriter.WriteSnapshot(network.TakeSnapshot(), version, ct);
                }

                logger.LogInformation("Event handler finished for {EventName}", nameof(RoadNetworkChangesAccepted));
            });

        For<RebuildRoadNetworkSnapshot>()
            .UseRoadRegistryContext(store, snapshotReader, loggerFactory, enricher)
            .Handle(async (context, _, ct) =>
            {
                logger.LogInformation("Command handler started for {CommandName}", nameof(RebuildRoadNetworkSnapshot));

                if (snapshotFeatureToggle.FeatureEnabled)
                {
                    await mediator.Send(new RebuildRoadNetworkSnapshotSqsRequest { Request = new RebuildRoadNetworkSnapshotRequest() }, ct);
                }
                else
                {
                    var (network, version) = await context.RoadNetworks.GetWithVersion(false, ct);
                    await snapshotWriter.WriteSnapshot(network.TakeSnapshot(), version, ct);

                    var completedCommand = new RebuildRoadNetworkSnapshotCompleted
                    {
                        CurrentVersion = version
                    };

                    await new RoadNetworkCommandQueue(store, applicationMetadata)
                        .Write(new Command(completedCommand), ct);
                }

                logger.LogInformation("Command handler finished for {Command}", nameof(RebuildRoadNetworkSnapshot));
            });

        For<RebuildRoadNetworkSnapshotCompleted>()
            .Handle((_, _) =>
            {
                logger.LogInformation("Command handler started for {CommandName}", nameof(RebuildRoadNetworkSnapshotCompleted));
                logger.LogInformation("Command handler finished for {Command}", nameof(RebuildRoadNetworkSnapshotCompleted));
                return Task.CompletedTask;
            });
    }
}
