namespace RoadRegistry.Editor.ProjectionHost.EventProcessors;

using System;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Microsoft.Extensions.Logging;
using RoadRegistry.Editor.ProjectionHost;
using Schema;
using SqlStreamStore;

public class ChangeFeedEventProcessor : DbContextEventProcessor<EditorContext>
{
    public ChangeFeedEventProcessor(
        IStreamStore streamStore,
        DbContextEventProcessorProjections<ChangeFeedEventProcessor, EditorContext> projections,
        EnvelopeFactory envelopeFactory,
        Func<EditorContext> dbContextFactory,
        Scheduler scheduler,
        ILogger<ChangeFeedEventProcessor> logger)
        : base("roadregistry-editor-changefeed-projectionhost", streamStore, projections.Filter, envelopeFactory, projections.Resolver, dbContextFactory, scheduler, logger)
    {
    }
}
