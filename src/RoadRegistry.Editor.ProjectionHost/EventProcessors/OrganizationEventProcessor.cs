namespace RoadRegistry.Editor.ProjectionHost.EventProcessors;

using System;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Microsoft.Extensions.Logging;
using Schema;
using SqlStreamStore;

public class OrganizationEventProcessor : EditorContextEventProcessor
{
    public OrganizationEventProcessor(
        IStreamStore streamStore,
        DbContextEventProcessorProjections<OrganizationEventProcessor, EditorContext> projections,
        EnvelopeFactory envelopeFactory,
        Func<EditorContext> dbContextFactory,
        Scheduler scheduler,
        ILogger<OrganizationEventProcessor> logger)
        : base("roadregistry-editor-organization-projectionhost", streamStore, projections.Filter, envelopeFactory, projections.Resolver, dbContextFactory, scheduler, logger)
    {
    }
}
