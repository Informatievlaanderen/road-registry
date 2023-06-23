using System;

namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class RoadRegistryEnumDataTypeAttribute : Attribute
    {
        public RoadRegistryEnumDataTypeAttribute(Type enumType)
        {
            EnumType = enumType;
        }

        public Type EnumType { get; }
    }
}
