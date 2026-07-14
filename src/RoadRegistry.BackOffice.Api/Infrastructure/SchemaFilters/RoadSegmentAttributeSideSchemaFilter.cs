namespace RoadRegistry.BackOffice.Api.Infrastructure.SchemaFilters;

using RoadRegistry.RoadSegment.ValueObjects;

public class RoadSegmentAttributeSideSchemaFilter : EnumSchemaFilter<RoadSegmentAttributeSide>
{
    public RoadSegmentAttributeSideSchemaFilter()
        : base(RoadSegmentAttributeSide.All)
    {
    }
}
