namespace RoadRegistry.BackOffice.Api.Infrastructure.SchemaFilters;

public class RoadSegmentStatusV2SchemaFilter : EnumSchemaFilter<RoadSegmentStatusV2>
{
    public RoadSegmentStatusV2SchemaFilter()
        : base(RoadSegmentStatusV2.All)
    {
    }
}
