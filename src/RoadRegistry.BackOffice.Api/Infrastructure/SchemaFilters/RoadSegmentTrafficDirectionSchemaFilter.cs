namespace RoadRegistry.BackOffice.Api.Infrastructure.SchemaFilters;

public class RoadSegmentTrafficDirectionSchemaFilter : EnumSchemaFilter<RoadSegmentTrafficDirection>
{
    public RoadSegmentTrafficDirectionSchemaFilter()
        : base(RoadSegmentTrafficDirection.All)
    {
    }
}
