namespace RoadRegistry.BackOffice.Api.Infrastructure.SchemaFilters;

public class OutlinedRoadSegmentMorphologySchemaFilter : EnumSchemaFilter<RoadSegmentMorphology.Outlined, RoadSegmentMorphology>
{
    public OutlinedRoadSegmentMorphologySchemaFilter()
        : base(RoadSegmentMorphology.Outlined.AllOutlined)
    {
    }
}
