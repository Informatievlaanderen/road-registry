namespace RoadRegistry.BackOffice.Infrastructure.Converters;

using System;
using Newtonsoft.Json;

public class GradeSeparatedJunctionIdConverter : NullableJsonConverter<GradeSeparatedJunctionId>
{
    public override GradeSeparatedJunctionId ReadJson(JsonReader reader, Type objectType, GradeSeparatedJunctionId? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return new GradeSeparatedJunctionId(Convert.ToInt32(reader.Value));
    }

    public override void WriteJson(JsonWriter writer, GradeSeparatedJunctionId value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToInt32());
    }
}
