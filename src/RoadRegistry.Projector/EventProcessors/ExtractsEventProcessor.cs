namespace RoadRegistry.Projector.EventProcessors;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Hosts;
using SqlStreamStore;

public class ExtractsEventProcessor : RunnerDbContextEventProcessor<ExtractsDbContext>
{
    public ExtractsEventProcessor(
        IStreamStore streamStore,
        DbContextEventProcessorProjections<ExtractsEventProcessor, ExtractsDbContext> projections,
        EnvelopeFactory envelopeFactory,
        IDbContextFactory<ExtractsDbContext> dbContextFactory,
        Scheduler scheduler,
        ILoggerFactory loggerFactory,
        IConfiguration configuration)
        : base("roadregistry-extracts", streamStore, projections.Filter, envelopeFactory, projections.Resolver, dbContextFactory, scheduler, loggerFactory, configuration)
    {
    }
}
