namespace RoadRegistry.MartenMigration.Projections;

using System;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SqlStreamStore;

public class MartenMigrationContextEventProcessor : RunnerDbContextEventProcessor<MartenMigrationContext>
{
    public MartenMigrationContextEventProcessor(
        IStreamStore streamStore,
        DbContextEventProcessorProjections<MartenMigrationContextEventProcessor, MartenMigrationContext> projections,
        EnvelopeFactory envelopeFactory,
        IDbContextFactory<MartenMigrationContext> dbContextFactory,
        Scheduler scheduler,
        ILoggerFactory loggerFactory,
        IConfiguration configuration)
        : base("roadregistry-martenmigration", streamStore, projections.Filter, envelopeFactory, projections.Resolver, dbContextFactory, scheduler, loggerFactory, configuration)
    {
    }
}
