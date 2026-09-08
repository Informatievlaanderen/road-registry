namespace RoadRegistry.BackOffice.Api.Infrastructure.SchemaFilters;

using RoadRegistry.ValueObjects;

public class EditGradeSeparatedJunctionTypeV2SchemaFilter : EnumSchemaFilter<GradeSeparatedJunctionTypeV2.Edit, GradeSeparatedJunctionTypeV2>
{
    public EditGradeSeparatedJunctionTypeV2SchemaFilter()
        : base(GradeSeparatedJunctionTypeV2.Edit.Values)
    {
    }
}
