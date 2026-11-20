using System;

namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers.Attributes
{
    /// <summary>
    ///     Documents a property as required and not nullable in the OpenAPI schema, without making the JSON deserializer
    ///     enforce it. Use it for a property the endpoint validates itself, so a missing value keeps getting the endpoint's
    ///     own problem detail instead of a generic invalid-JSON error.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class DocumentedAsRequiredAttribute : Attribute
    {
    }
}
