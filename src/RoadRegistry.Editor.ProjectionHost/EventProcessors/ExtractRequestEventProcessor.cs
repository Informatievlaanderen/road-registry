namespace RoadRegistry.Editor.ProjectionHost.EventProcessors;

using System;
using BackOffice;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Schema;
using SqlStreamStore;

public class ExtractRequestEventProcessor : EditorContextEventProcessor
{
    public ExtractRequestEventProcessor(
        IStreamStore streamStore,
        DbContextEventProcessorProjections<ExtractRequestEventProcessor, EditorContext> projections,
        EnvelopeFactory envelopeFactory,
        Func<EditorContext> dbContextFactory,
        Scheduler scheduler,
        ILoggerFactory loggerFactory,
        IConfiguration configuration)
        : base(WellKnownProjectionStateNames.RoadRegistryEditorExtractRequestProjectionHost, streamStore, projections.Filter, envelopeFactory, projections.Resolver, dbContextFactory, scheduler, loggerFactory, configuration)
    {
    }
}
