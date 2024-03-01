namespace RoadRegistry.BackOffice.Api.Infrastructure.SchemaFilters;

public class RoadSegmentNumberedRoadDirectionSchemaFilter : EnumSchemaFilter<RoadSegmentNumberedRoadDirection>
{
    public RoadSegmentNumberedRoadDirectionSchemaFilter()
        : base(RoadSegmentNumberedRoadDirection.All)
    {
    }
}
