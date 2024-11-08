namespace RoadRegistry.Editor.ProjectionHost.EventProcessors;

using System;
using BackOffice;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Microsoft.Extensions.Logging;
using Schema;
using SqlStreamStore;

public class RoadNetworkEventProcessor : EditorContextEventProcessor
{
    public RoadNetworkEventProcessor(
        IStreamStore streamStore,
        DbContextEventProcessorProjections<RoadNetworkEventProcessor, EditorContext> projections,
        EnvelopeFactory envelopeFactory,
        Func<EditorContext> dbContextFactory,
        Scheduler scheduler,
        ILoggerFactory loggerFactory)
        : base(WellKnownProjectionStateNames.RoadRegistryEditorRoadNetworkProjectionHost, streamStore, projections.Filter, envelopeFactory, projections.Resolver, dbContextFactory, scheduler, loggerFactory)
    {
    }
}
