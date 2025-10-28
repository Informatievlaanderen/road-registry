namespace RoadRegistry.BackOffice.Infrastructure.Converters;

using System;
using Newtonsoft.Json;

public class OrganizationIdConverter : NullableJsonConverter<OrganizationId>
{
    public override OrganizationId ReadJson(JsonReader reader, Type objectType, OrganizationId? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return new OrganizationId(reader.Value?.ToString());
    }

    public override void WriteJson(JsonWriter writer, OrganizationId value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
