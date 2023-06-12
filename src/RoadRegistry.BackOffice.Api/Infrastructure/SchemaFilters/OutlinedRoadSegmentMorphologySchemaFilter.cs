namespace RoadRegistry.BackOffice.Api.Infrastructure.SchemaFilters;

public class OutlinedRoadSegmentMorphologySchemaFilter : EnumSchemaFilter<RoadSegmentMorphology.Edit, RoadSegmentMorphology>
{
    public OutlinedRoadSegmentMorphologySchemaFilter()
        : base(RoadSegmentMorphology.Edit.Editable)
    {
    }
}
