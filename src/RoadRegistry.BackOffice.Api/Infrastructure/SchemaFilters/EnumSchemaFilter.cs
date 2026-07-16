using System.Collections.Generic;
using System;
using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace RoadRegistry.BackOffice.Api.Infrastructure.SchemaFilters
{
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using Controllers.Attributes;

    public abstract class EnumSchemaFilter<T> : EnumSchemaFilter<T, T>
        where T : IDutchToString
    {
        protected EnumSchemaFilter(IEnumerable<T> items)
            : base(items)
        {
        }
    }

    public abstract class EnumSchemaFilter<TEnum, TValues> : ISchemaFilter
        where TValues : IDutchToString
    {
        private readonly IEnumerable<TValues> _items;

        protected EnumSchemaFilter(IEnumerable<TValues> items)
        {
            _items = items;
        }

        public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
        {
            var enumDataTypeAttribute = context.MemberInfo?.CustomAttributes.SingleOrDefault(x => x.AttributeType == typeof(EnumDataTypeAttribute)
                                                                                                  || x.AttributeType == typeof(RoadRegistryEnumDataTypeAttribute));
            if (enumDataTypeAttribute is not null && schema is OpenApiSchema openApiSchema)
            {
                var enumType = (Type)enumDataTypeAttribute.ConstructorArguments.Single().Value;
                if (enumType == typeof(TEnum))
                {
                    openApiSchema.Enum = _items
                        .Select(x => JsonValue.Create(x.ToDutchString()) as JsonNode)
                        .OrderBy(x => x!.GetValue<string>())
                        .ToList();
                }
            }
        }
    }
}
