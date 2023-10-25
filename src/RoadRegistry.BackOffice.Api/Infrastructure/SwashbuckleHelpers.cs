namespace RoadRegistry.BackOffice.Api.Infrastructure
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Attributes;

    public static class SwashbuckleHelpers
    {
        /// <summary>
        /// Direct copy of <see href="https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/master/src/Swashbuckle.AspNetCore.SwaggerGen/SchemaGenerator/SchemaGeneratorOptions.cs">Swashbuckle.AspNetCore.SwaggerGen.SchemaGeneratorOptions.DefaultSchemaIdSelector</see>.
        /// </summary>
        public static string DefaultSchemaIdSelector(Type modelType)
        {
            if (!modelType.IsConstructedGenericType) return modelType.Name.Replace("[]", "Array");

            var prefix = modelType.GetGenericArguments()
                .Select(DefaultSchemaIdSelector)
                .Aggregate((previous, current) => previous + current);

            return prefix + modelType.Name.Split('`').First();
        }

        public static string GetCustomSchemaId(Type modelType)
        {
            return modelType.GetCustomAttributes<CustomSwaggerSchemaIdAttribute>().SingleOrDefault()?.SchemaId;
        }
    }
}
