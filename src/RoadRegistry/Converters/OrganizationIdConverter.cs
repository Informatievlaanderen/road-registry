namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class OrganizationIdConverter : NullableValueTypeJsonConverter<OrganizationId>
{
    protected override OrganizationId ReadJson(object value, Type objectType, JsonSerializer serializer)
    {
        return new OrganizationId((string)value);
    }

    protected override void WriteJson(JsonWriter writer, OrganizationId value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
