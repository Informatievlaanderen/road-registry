namespace RoadRegistry.BackOffice.Api.Infrastructure.SchemaFilters;

public class RoadSegmentGeometryDrawMethodSchemaFilter : EnumSchemaFilter<RoadSegmentGeometryDrawMethod>
{
    public RoadSegmentGeometryDrawMethodSchemaFilter()
        : base(RoadSegmentGeometryDrawMethod.Allowed)
    {
    }
}
