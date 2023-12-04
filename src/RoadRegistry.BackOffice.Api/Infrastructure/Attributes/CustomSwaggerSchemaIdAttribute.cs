using System;

namespace RoadRegistry.BackOffice.Api.Infrastructure.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class CustomSwaggerSchemaIdAttribute : Attribute
    {
        public string SchemaId { get; }

        public CustomSwaggerSchemaIdAttribute(string schemaId)
        {
            SchemaId = schemaId;
        }
    }
}
