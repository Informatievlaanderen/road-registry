// namespace RoadRegistry.Integration.ProjectionHost.EventProcessors;
//
// using System;
// using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
// using Hosts;
// using Microsoft.Extensions.Logging;
// using Schema;
// using SqlStreamStore;
//
// public class OrganizationEventProcessor : IntegrationContextEventProcessor
// {
//     public OrganizationEventProcessor(
//         IStreamStore streamStore,
//         DbContextEventProcessorProjections<OrganizationEventProcessor, IntegrationContext> projections,
//         EnvelopeFactory envelopeFactory,
//         Func<IntegrationContext> dbContextFactory,
//         Scheduler scheduler,
//         ILogger<OrganizationEventProcessor> logger)
//         : base("roadregistry-integration-organization-projectionhost", streamStore, projections.Filter, envelopeFactory, projections.Resolver, dbContextFactory, scheduler, logger)
//     {
//     }
// }
