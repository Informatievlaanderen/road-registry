using Microsoft.OpenApi.Any;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System;

namespace RoadRegistry.BackOffice.Api.Infrastructure.SchemaFilters
{
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using Microsoft.OpenApi.Models;

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

        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            var enumDataTypeAttribute = context.MemberInfo?.CustomAttributes.SingleOrDefault(x => x.AttributeType == typeof(EnumDataTypeAttribute));
            if (enumDataTypeAttribute is not null)
            {
                var enumType = (Type)enumDataTypeAttribute.ConstructorArguments.Single().Value;
                if (enumType == typeof(TEnum))
                {
                    schema.Enum = _items
                        .Select(x => new OpenApiString(x.ToDutchString()))
                        .OrderBy(x => x.Value)
                        .Cast<IOpenApiAny>()
                        .ToList();
                }
            }
        }
    }
}
