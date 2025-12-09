namespace RoadRegistry.Extracts.Projections;

using Infrastructure.MartenDb.Projections;
using Marten;

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
