namespace RoadRegistry.BackOffice.Api.Infrastructure.Extensions;

using Microsoft.Extensions.DependencyInjection;
using SchemaFilters;
using Swashbuckle.AspNetCore.SwaggerGen;

public static class SwaggerExtensions
{
    public static void AddRoadRegistrySchemaFilters(this SwaggerGenOptions options)
    {
        options.SchemaFilter<OutlinedRoadSegmentMorphologySchemaFilter>();
        options.SchemaFilter<OutlinedRoadSegmentStatusSchemaFilter>();
        options.SchemaFilter<RoadSegmentAccessRestrictionSchemaFilter>();
        options.SchemaFilter<RoadSegmentCategorySchemaFilter>();
        options.SchemaFilter<RoadSegmentGeometryDrawMethodSchemaFilter>();
        options.SchemaFilter<RoadSegmentLaneDirectionSchemaFilter>();
        options.SchemaFilter<RoadSegmentMorphologySchemaFilter>();
        options.SchemaFilter<RoadSegmentStatusSchemaFilter>();
        options.SchemaFilter<RoadSegmentSurfaceTypeSchemaFilter>();
    }
}
