namespace RoadRegistry.Integration.ProjectionHost.EventProcessors;

using System;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Microsoft.Extensions.Logging;
using Schema;
using SqlStreamStore;

public class OrganizationVersionEventProcessor : IntegrationContextEventProcessor
{
    public OrganizationVersionEventProcessor(
        IStreamStore streamStore,
        DbContextEventProcessorProjections<OrganizationVersionEventProcessor, IntegrationContext> projections,
        EnvelopeFactory envelopeFactory,
        Func<IntegrationContext> dbContextFactory,
        Scheduler scheduler,
        ILoggerFactory loggerFactory)
        : base(
            "roadregistry-integration-organization-version-projectionhost",
            streamStore,
            projections.Filter,
            envelopeFactory,
            projections.Resolver,
            dbContextFactory,
            scheduler,
            loggerFactory)
    {
    }
}
