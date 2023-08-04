namespace RoadRegistry.Editor.ProjectionHost.EventProcessors;

using System;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Microsoft.Extensions.Logging;
using Schema;
using SqlStreamStore;

public class ExtractUploadEventProcessor : EditorContextEventProcessor
{
    public ExtractUploadEventProcessor(
        IStreamStore streamStore,
        EditorContextEventProcessorProjections<ExtractUploadEventProcessor> projections,
        EnvelopeFactory envelopeFactory,
        Func<EditorContext> dbContextFactory,
        Scheduler scheduler,
        ILogger<ExtractUploadEventProcessor> logger)
        : base("roadregistry-editor-extractupload-projectionhost", streamStore, projections.Filter, envelopeFactory, projections.Resolver, dbContextFactory, scheduler, logger)
    {
    }
}
