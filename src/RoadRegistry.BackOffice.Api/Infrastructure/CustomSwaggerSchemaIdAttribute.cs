namespace RoadRegistry.BackOffice.Api.Infrastructure
{
    using System;

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
