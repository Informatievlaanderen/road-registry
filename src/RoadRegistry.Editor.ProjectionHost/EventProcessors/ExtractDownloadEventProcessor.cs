namespace RoadRegistry.Editor.ProjectionHost.EventProcessors;

using System;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Schema;
using SqlStreamStore;

public class ExtractDownloadEventProcessor : EditorContextEventProcessor
{
    public ExtractDownloadEventProcessor(
        IStreamStore streamStore,
        DbContextEventProcessorProjections<ExtractDownloadEventProcessor, EditorContext> projections,
        EnvelopeFactory envelopeFactory,
        Func<EditorContext> dbContextFactory,
        Scheduler scheduler,
        ILoggerFactory loggerFactory,
        IConfiguration configuration)
        : base("roadregistry-editor-extractdownload-projectionhost", streamStore, projections.Filter, envelopeFactory, projections.Resolver, dbContextFactory, scheduler, loggerFactory, configuration)
    {
    }
}
