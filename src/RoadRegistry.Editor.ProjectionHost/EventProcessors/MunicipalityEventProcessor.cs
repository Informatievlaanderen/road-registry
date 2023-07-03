namespace RoadRegistry.Editor.ProjectionHost.EventProcessors;

using System;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Microsoft.Extensions.Logging;
using RoadRegistry.Editor.ProjectionHost;
using Schema;
using SqlStreamStore;

public class MunicipalityEventProcessor : DbContextEventProcessor<EditorContext>
{
    public MunicipalityEventProcessor(
        IStreamStore streamStore,
        DbContextEventProcessorProjections<MunicipalityEventProcessor, EditorContext> projections,
        EnvelopeFactory envelopeFactory,
        Func<EditorContext> dbContextFactory,
        Scheduler scheduler,
        ILogger<MunicipalityEventProcessor> logger)
        : base("roadregistry-editor-municipality-projectionhost", streamStore, projections.Filter, envelopeFactory, projections.Resolver, dbContextFactory, scheduler, logger)
    {
    }
}
