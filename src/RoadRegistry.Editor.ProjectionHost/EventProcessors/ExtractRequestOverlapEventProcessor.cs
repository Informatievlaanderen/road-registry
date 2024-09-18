namespace RoadRegistry.Editor.ProjectionHost.EventProcessors;

using System;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Microsoft.Extensions.Logging;
using Schema;
using SqlStreamStore;

public class ExtractRequestOverlapEventProcessor : EditorContextEventProcessor
{
    public ExtractRequestOverlapEventProcessor(
        IStreamStore streamStore,
        DbContextEventProcessorProjections<ExtractRequestOverlapEventProcessor, EditorContext> projections,
        EnvelopeFactory envelopeFactory,
        Func<EditorContext> dbContextFactory,
        Scheduler scheduler,
        ILogger<ExtractRequestOverlapEventProcessor> logger)
        : base("roadregistry-editor-extractrequestoverlap-projectionhost", streamStore, projections.Filter, envelopeFactory, projections.Resolver, dbContextFactory, scheduler, logger, catchUpBatchSize: 1)
    {
    }
}
