namespace RoadRegistry.Editor.ProjectionHost.EventProcessors;

using System;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Microsoft.Extensions.Logging;
using Schema;
using SqlStreamStore;

public class ExtractRequestEventProcessor : EditorContextEventProcessor
{
    public ExtractRequestEventProcessor(
        IStreamStore streamStore,
        EditorContextEventProcessorProjections<ExtractRequestEventProcessor> projections,
        EnvelopeFactory envelopeFactory,
        Func<EditorContext> dbContextFactory,
        Scheduler scheduler,
        ILogger<ExtractRequestEventProcessor> logger)
        : base("roadregistry-editor-extractrequest-projectionhost", streamStore, projections.Filter, envelopeFactory, projections.Resolver, dbContextFactory, scheduler, logger)
    {
    }
}
