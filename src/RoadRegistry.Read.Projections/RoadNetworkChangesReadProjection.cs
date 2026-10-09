namespace RoadRegistry.Read.Projections;

using Marten;
using Microsoft.Extensions.Logging;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadRegistry.Read.Projections.Setup;
using RoadRegistry.StreetName;

public class RoadNetworkChangesReadProjection : MartenBackedRoadNetworkChangesProjection
{
    // No read document points at another one any more: every handler writes the rows of the entity its event belongs
    // to and reads the others only by their own key, so two events of one change never compete for a row and a
    // segment no longer has to wait for the node it references. That is what makes the order a correlation's events
    // arrive in irrelevant - and with it the tail fetch that pulled a correlation's later events into the batch, and
    // the per-correlation progression that guarded them. See RoadNetworkChangesProjection.RequiresEmissionOrder.
    protected override bool RequiresEmissionOrder => false;

    public RoadNetworkChangesReadProjection(int batchSize, ILoggerFactory loggerFactory, IStreetNameClient streetNameClient)
        : base([
                new OrganizationReadProjection(),
                new StreetNameReadProjection(),
                new RoadNodeReadProjection(),
                new RoadSegmentReadProjection(streetNameClient, loggerFactory.CreateLogger<RoadSegmentReadProjection>()),
                new GradeSeparatedJunctionReadProjection(),
                new GradeJunctionReadProjection()
            ], loggerFactory,
            batchSize: batchSize)
    {
    }

    protected override void ConfigureSchema(StoreOptions options)
    {
        options.ConfigureReadDocuments();
    }
}
