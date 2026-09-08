namespace RoadRegistry.BackOffice.Api.Infrastructure.SchemaFilters;

using RoadRegistry.ValueObjects;

// Every kind a grade separated junction can be recorded as, 'nietGekend' included: what a reader may get back is not
// the same set as what a caller may ask for, which is what EditGradeSeparatedJunctionTypeV2SchemaFilter documents.
public class GradeSeparatedJunctionTypeV2SchemaFilter : EnumSchemaFilter<GradeSeparatedJunctionTypeV2>
{
    public GradeSeparatedJunctionTypeV2SchemaFilter()
        : base(GradeSeparatedJunctionTypeV2.All)
    {
    }
}
