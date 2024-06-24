namespace RoadRegistry.Integration.ProjectionHost.EventProcessors;

using System;
using BackOffice;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Microsoft.Extensions.Logging;
using Schema;
using SqlStreamStore;

public class RoadNetworkLatestItemEventProcessor : IntegrationContextEventProcessor
{
    public RoadNetworkLatestItemEventProcessor(
        IStreamStore streamStore,
        DbContextEventProcessorProjections<RoadNetworkLatestItemEventProcessor, IntegrationContext> projections,
        EnvelopeFactory envelopeFactory,
        Func<IntegrationContext> dbContextFactory,
        Scheduler scheduler,
        ILogger<RoadNetworkLatestItemEventProcessor> logger)
        : base(
            "roadregistry-integration-roadnetwork-latestitem-projectionhost",
            streamStore,
            projections.Filter,
            envelopeFactory,
            projections.Resolver,
            dbContextFactory,
            scheduler,
            logger)
    {
    }
}
