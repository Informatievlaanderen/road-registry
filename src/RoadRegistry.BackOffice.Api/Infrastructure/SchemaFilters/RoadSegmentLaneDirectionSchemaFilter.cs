namespace RoadRegistry.BackOffice.Api.Infrastructure.SchemaFilters;

public class RoadSegmentLaneDirectionSchemaFilter : EnumSchemaFilter<RoadSegmentLaneDirection>
{
    public RoadSegmentLaneDirectionSchemaFilter()
        : base(RoadSegmentLaneDirection.All)
    {
    }
}
