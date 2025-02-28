namespace RoadRegistry.Product.ProjectionHost.EventProcessors;

using System;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Schema;
using SqlStreamStore;

public class OrganizationV2EventProcessor : ProductContextEventProcessor
{
    public OrganizationV2EventProcessor(
        IStreamStore streamStore,
        DbContextEventProcessorProjections<OrganizationV2EventProcessor, ProductContext> projections,
        EnvelopeFactory envelopeFactory,
        Func<ProductContext> dbContextFactory,
        Scheduler scheduler,
        ILoggerFactory loggerFactory,
        IConfiguration configuration)
        : base("roadregistry-product-organization-v2-projectionhost", streamStore, projections.Filter, envelopeFactory, projections.Resolver, dbContextFactory, scheduler, loggerFactory, configuration)
    {
    }
}
