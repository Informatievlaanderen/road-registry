namespace RoadRegistry.Integration.ProjectionHost.EventProcessors;

using System;
using BackOffice;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Microsoft.Extensions.Configuration;
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
        ILoggerFactory loggerFactory,
        IConfiguration configuration)
        : base(
            "roadregistry-integration-roadnetwork-latestitem-projectionhost",
            streamStore,
            projections.Filter,
            envelopeFactory,
            projections.Resolver,
            dbContextFactory,
            scheduler,
            loggerFactory,
            configuration)
    {
    }
}
