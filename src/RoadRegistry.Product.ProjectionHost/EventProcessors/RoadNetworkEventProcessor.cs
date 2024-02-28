namespace RoadRegistry.Product.ProjectionHost.EventProcessors;

using System;
using BackOffice;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Microsoft.Extensions.Logging;
using Schema;
using SqlStreamStore;

public class RoadNetworkEventProcessor : ProductContextEventProcessor
{
    public RoadNetworkEventProcessor(
        IStreamStore streamStore,
        DbContextEventProcessorProjections<RoadNetworkEventProcessor, ProductContext> projections,
        EnvelopeFactory envelopeFactory,
        Func<ProductContext> dbContextFactory,
        Scheduler scheduler,
        ILogger<RoadNetworkEventProcessor> logger)
        : base(WellKnownProjectionStateNames.RoadRegistryProductRoadNetworkProjectionHost, streamStore, projections.Filter, envelopeFactory, projections.Resolver, dbContextFactory, scheduler, logger)
    {
    }
}
