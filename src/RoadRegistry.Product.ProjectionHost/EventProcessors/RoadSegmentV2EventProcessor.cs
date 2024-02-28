namespace RoadRegistry.Product.ProjectionHost.EventProcessors;

using BackOffice;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Microsoft.Extensions.Logging;
using Schema;
using SqlStreamStore;
using System;

public class RoadSegmentV2EventProcessor : ProductContextEventProcessor
{
    public RoadSegmentV2EventProcessor(
        IStreamStore streamStore,
        DbContextEventProcessorProjections<RoadSegmentV2EventProcessor, ProductContext> projections,
        EnvelopeFactory envelopeFactory,
        Func<ProductContext> dbContextFactory,
        Scheduler scheduler,
        ILogger<RoadSegmentV2EventProcessor> logger)
        : base(WellKnownProjectionStateNames.RoadRegistryProductRoadSegmentV2ProjectionHost, streamStore, projections.Filter, envelopeFactory, projections.Resolver, dbContextFactory, scheduler, logger)
    {
    }
}
