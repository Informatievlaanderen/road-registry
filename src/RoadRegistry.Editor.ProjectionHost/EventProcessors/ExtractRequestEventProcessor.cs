namespace RoadRegistry.Editor.ProjectionHost.EventProcessors;

using System;
using BackOffice.FeatureToggles;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
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
        UseExtractRequestOverlapEventProcessorFeatureToggle useExtractRequestOverlapEventProcessorFeatureToggle,
        ILoggerFactory loggerFactory)
        : base("roadregistry-editor-extractrequest-projectionhost", streamStore, projections.Filter, envelopeFactory, projections.Resolver, dbContextFactory, scheduler, loggerFactory, catchUpBatchSize: useExtractRequestOverlapEventProcessorFeatureToggle.FeatureEnabled ? 500 : 1)
    {
    }
}
