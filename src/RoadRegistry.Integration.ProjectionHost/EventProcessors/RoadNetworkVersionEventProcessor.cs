namespace RoadRegistry.Integration.ProjectionHost.EventProcessors;

using System;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Microsoft.Extensions.Logging;
using Schema;
using SqlStreamStore;

public class RoadNetworkVersionEventProcessor : IntegrationContextEventProcessor
{
    public RoadNetworkVersionEventProcessor(
        IStreamStore streamStore,
        DbContextEventProcessorProjections<RoadNetworkVersionEventProcessor, IntegrationContext> projections,
        EnvelopeFactory envelopeFactory,
        Func<IntegrationContext> dbContextFactory,
        Scheduler scheduler,
        ILogger<RoadNetworkVersionEventProcessor> logger)
        : base(
            "roadregistry-integration-roadnetwork-version-projectionhost",
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
