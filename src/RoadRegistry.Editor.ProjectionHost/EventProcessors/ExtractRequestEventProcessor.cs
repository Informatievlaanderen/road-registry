namespace RoadRegistry.Editor.ProjectionHost.EventProcessors;

using System;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Microsoft.Extensions.Logging;
using RoadRegistry.Editor.ProjectionHost;
using Schema;
using SqlStreamStore;

public class ExtractRequestEventProcessor : DbContextEventProcessor<EditorContext>
{
    public ExtractRequestEventProcessor(
        IStreamStore streamStore,
        DbContextEventProcessorProjections<ExtractRequestEventProcessor, EditorContext> projections,
        EnvelopeFactory envelopeFactory,
        Func<EditorContext> dbContextFactory,
        Scheduler scheduler,
        ILogger<ExtractRequestEventProcessor> logger)
        : base("roadregistry-editor-extractrequest-projectionhost", streamStore, projections.Filter, envelopeFactory, projections.Resolver, dbContextFactory, scheduler, logger)
    {
    }
}
