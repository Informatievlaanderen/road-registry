namespace RoadRegistry.Extracts.Projections;

using Marten;
using RoadRegistry.Infrastructure.MartenDb.Projections;

public class ExtractsRoadNetworkChangesProjection : RoadNetworkChangesProjection
{
    public ExtractsRoadNetworkChangesProjection()
        : base([new RoadNodeProjection(), new RoadSegmentProjection(), new GradeSeparatedJunctionProjection()])
    {
    }

    public override void Configure(StoreOptions options)
    {
        RoadNodeProjection.Configure(options);
        RoadSegmentProjection.Configure(options);
        GradeSeparatedJunctionProjection.Configure(options);
    }
}
