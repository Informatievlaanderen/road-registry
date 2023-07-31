namespace RoadRegistry.Editor.ProjectionHost.EventProcessors;

using System;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Microsoft.Extensions.Logging;
using Schema;
using SqlStreamStore;

public class MunicipalityEventProcessor : EditorContextEventProcessor
{
    public MunicipalityEventProcessor(
        IStreamStore streamStore,
        EditorContextEventProcessorProjections<MunicipalityEventProcessor> projections,
        EnvelopeFactory envelopeFactory,
        Func<EditorContext> dbContextFactory,
        Scheduler scheduler,
        ILogger<MunicipalityEventProcessor> logger)
        : base("roadregistry-editor-municipality-projectionhost", streamStore, projections.Filter, envelopeFactory, projections.Resolver, dbContextFactory, scheduler, logger)
    {
    }
}
