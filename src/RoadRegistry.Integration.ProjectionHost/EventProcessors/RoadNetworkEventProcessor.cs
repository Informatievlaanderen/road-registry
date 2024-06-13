namespace RoadRegistry.Integration.ProjectionHost.EventProcessors;

using System;
using BackOffice;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Microsoft.Extensions.Logging;
using Schema;
using SqlStreamStore;

public class RoadNetworkEventProcessor : IntegrationContextEventProcessor
{
    public RoadNetworkEventProcessor(
        IStreamStore streamStore,
        DbContextEventProcessorProjections<RoadNetworkEventProcessor, IntegrationContext> projections,
        EnvelopeFactory envelopeFactory,
        Func<IntegrationContext> dbContextFactory,
        Scheduler scheduler,
        ILogger<RoadNetworkEventProcessor> logger)
        : base(
            WellKnownProjectionStateNames.RoadRegistryIntegrationRoadNetworkProjectionHost,
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
