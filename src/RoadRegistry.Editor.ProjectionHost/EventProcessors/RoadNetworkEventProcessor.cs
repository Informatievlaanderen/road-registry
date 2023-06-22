namespace RoadRegistry.Editor.ProjectionHost.EventProcessors;

using System;
using BackOffice;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Microsoft.Extensions.Logging;
using RoadRegistry.Editor.ProjectionHost;
using Schema;
using SqlStreamStore;

public class RoadNetworkEventProcessor : DbContextEventProcessor<EditorContext>
{
    private const string QueueName = WellKnownProjectionStateNames.RoadRegistryEditorRoadNetworkProjectionHost;

    public RoadNetworkEventProcessor(
        IStreamStore streamStore,
        DbContextEventProcessorProjections<RoadNetworkEventProcessor, EditorContext> projections,
        EnvelopeFactory envelopeFactory,
        Func<EditorContext> dbContextFactory,
        Scheduler scheduler,
        ILogger<RoadNetworkEventProcessor> logger)
        : base(QueueName, streamStore, projections.Filter, envelopeFactory, projections.Resolver, dbContextFactory, scheduler, logger)
    {
    }
}
