namespace RoadRegistry.Editor.ProjectionHost.EventProcessors;

using System;
using BackOffice;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Microsoft.Extensions.Logging;
using Schema;
using SqlStreamStore;

public class OrganizationV2EventProcessor : EditorContextEventProcessor
{
    public OrganizationV2EventProcessor(
        IStreamStore streamStore,
        DbContextEventProcessorProjections<OrganizationV2EventProcessor, EditorContext> projections,
        EnvelopeFactory envelopeFactory,
        Func<EditorContext> dbContextFactory,
        Scheduler scheduler,
        ILogger<OrganizationV2EventProcessor> logger)
        : base(WellKnownProjectionStateNames.RoadRegistryEditorOrganizationV2ProjectionHost, streamStore, projections.Filter, envelopeFactory, projections.Resolver, dbContextFactory, scheduler, logger)
    {
    }
}