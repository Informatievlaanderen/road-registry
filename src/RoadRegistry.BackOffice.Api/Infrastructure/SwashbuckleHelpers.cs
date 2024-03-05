namespace RoadRegistry.BackOffice.Api.Infrastructure
{
    using System;
    using System.Linq;
    using System.Reflection;

    public static class SwashbuckleHelpers
    {
        public static string PublicApiDefaultSchemaIdSelector(Type modelType)
        {
            return modelType.Name;
        }

        public static string GetCustomSchemaId(Type modelType)
        {
            // Do NOT use DataContract here, it will give conflict in PublicApi
            return modelType.GetCustomAttributes<CustomSwaggerSchemaIdAttribute>().SingleOrDefault()?.SchemaId;
        }
    }
}
