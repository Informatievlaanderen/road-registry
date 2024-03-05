namespace RoadRegistry.BackOffice.Api.Infrastructure
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;

    public static class SwashbuckleHelpers
    {
        public static string PublicApiDefaultSchemaIdSelector(Type modelType)
        {
            return modelType.Name;
        }

        public static string GetCustomSchemaId(Type modelType)
        {
            return modelType.GetCustomAttributes<DataContractAttribute>().SingleOrDefault()?.Name;
        }
    }
}
