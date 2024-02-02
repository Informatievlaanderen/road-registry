namespace RoadRegistry.Editor.ProjectionHost.EventProcessors;

using System;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice;
using Schema;
using SqlStreamStore;

public class RoadSegmentV2EventProcessor : EditorContextEventProcessor
{
    public RoadSegmentV2EventProcessor(
        IStreamStore streamStore,
        DbContextEventProcessorProjections<RoadSegmentV2EventProcessor, EditorContext> projections,
        EnvelopeFactory envelopeFactory,
        Func<EditorContext> dbContextFactory,
        Scheduler scheduler,
        ILogger<RoadSegmentV2EventProcessor> logger)
        : base(WellKnownProjectionStateNames.RoadRegistryEditorRoadSegmentV2ProjectionHost, streamStore, projections.Filter, envelopeFactory, projections.Resolver, dbContextFactory, scheduler, logger)
    {
    }
}
