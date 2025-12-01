namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class RoadNodeIdConverter : NullableJsonConverter<RoadNodeId>
{
    public override RoadNodeId ReadJson(JsonReader reader, Type objectType, RoadNodeId? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return new RoadNodeId(Convert.ToInt32(reader.Value));
    }

    public override void WriteJson(JsonWriter writer, RoadNodeId value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToInt32());
    }
}

