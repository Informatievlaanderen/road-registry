namespace RoadRegistry.Integration.ProjectionHost.EventProcessors;

using System;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Microsoft.Extensions.Logging;
using Schema;
using SqlStreamStore;

public class OrganizationLatestItemEventProcessor : IntegrationContextEventProcessor
{
    public OrganizationLatestItemEventProcessor(
        IStreamStore streamStore,
        DbContextEventProcessorProjections<OrganizationLatestItemEventProcessor, IntegrationContext> projections,
        EnvelopeFactory envelopeFactory,
        Func<IntegrationContext> dbContextFactory,
        Scheduler scheduler,
        ILoggerFactory loggerFactory)
        : base(
            "roadregistry-integration-organization-latestitem-projectionhost",
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
