namespace RoadRegistry.BackOffice.Api.Infrastructure.SchemaFilters;

public class RoadSegmentTrafficDirectionPedestrianSchemaFilter : EnumSchemaFilter<RoadSegmentPedestrianTrafficDirection>
{
    public RoadSegmentTrafficDirectionPedestrianSchemaFilter()
        : base(RoadSegmentPedestrianTrafficDirection.All)
    {
    }
}
