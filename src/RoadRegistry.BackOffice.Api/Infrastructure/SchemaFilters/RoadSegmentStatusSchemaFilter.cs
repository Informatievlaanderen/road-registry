namespace RoadRegistry.BackOffice.Api.Infrastructure.SchemaFilters;

public class RoadSegmentStatusSchemaFilter : EnumSchemaFilter<RoadSegmentStatus>
{
    public RoadSegmentStatusSchemaFilter()
        : base(RoadSegmentStatus.All)
    {
    }
}
