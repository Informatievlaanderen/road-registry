namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class GradeSeparatedJunctionTypeConverter : JsonConverter<GradeSeparatedJunctionType>
{
    public override GradeSeparatedJunctionType ReadJson(JsonReader reader, Type objectType, GradeSeparatedJunctionType existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return GradeSeparatedJunctionType.Parse(reader.Value?.ToString());
    }

    public override void WriteJson(JsonWriter writer, GradeSeparatedJunctionType value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
