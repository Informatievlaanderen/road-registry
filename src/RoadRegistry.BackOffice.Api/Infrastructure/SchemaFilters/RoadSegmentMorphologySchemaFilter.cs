namespace RoadRegistry.BackOffice.Api.Infrastructure.SchemaFilters;

public class RoadSegmentMorphologySchemaFilter : EnumSchemaFilter<RoadSegmentMorphology>
{
    public RoadSegmentMorphologySchemaFilter()
        : base(RoadSegmentMorphology.All)
    {
    }
}
