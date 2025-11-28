namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class RoadNodeTypeConverter : JsonConverter<RoadNodeType>
{
    public override RoadNodeType ReadJson(JsonReader reader, Type objectType, RoadNodeType existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return RoadNodeType.Parse(reader.Value?.ToString());
    }

    public override void WriteJson(JsonWriter writer, RoadNodeType value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
