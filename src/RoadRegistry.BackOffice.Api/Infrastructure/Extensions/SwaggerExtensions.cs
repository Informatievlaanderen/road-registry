namespace RoadRegistry.BackOffice.Api.Infrastructure.Extensions;

using Microsoft.Extensions.DependencyInjection;
using RoadRegistry.BackOffice.Api.Infrastructure;
using RoadRegistry.BackOffice.Api.Infrastructure.Attributes;
using SchemaFilters;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Reflection;

public static class SwaggerExtensions
{
    public static void AddRoadRegistrySchemaFilters(this SwaggerGenOptions options)
    {
        options.CustomSchemaIds(t => t.GetCustomAttributes<CustomSwaggerSchemaIdAttribute>().SingleOrDefault()?.SchemaId
                                     ?? SwashbuckleHelpers.DefaultSchemaIdSelector(t));
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
