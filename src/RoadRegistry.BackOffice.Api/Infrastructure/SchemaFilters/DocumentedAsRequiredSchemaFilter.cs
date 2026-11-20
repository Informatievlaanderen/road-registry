namespace RoadRegistry.BackOffice.Api.Infrastructure.SchemaFilters;

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Controllers.Attributes;
using Microsoft.OpenApi;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.SwaggerGen;

public class DocumentedAsRequiredSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is not OpenApiSchema openApiSchema || context.Type is null)
        {
            return;
        }

        var properties = context.Type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(x => x.GetCustomAttribute<DocumentedAsRequiredAttribute>() is not null);

        foreach (var property in properties)
        {
            var name = property.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName
                       ?? property.GetCustomAttribute<DataMemberAttribute>()?.Name
                       ?? property.Name;

            if (openApiSchema.Properties is null || !openApiSchema.Properties.TryGetValue(name, out var propertySchema))
            {
                continue;
            }

            openApiSchema.Required ??= new HashSet<string>();
            openApiSchema.Required.Add(name);

            if (propertySchema is OpenApiSchema openApiPropertySchema && openApiPropertySchema.Type is { } type)
            {
                openApiPropertySchema.Type = type & ~JsonSchemaType.Null;
            }
        }
    }
}
