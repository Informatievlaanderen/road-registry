namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class RoadSegmentLaneDirectionConverter : JsonConverter<RoadSegmentLaneDirection>
{
    public override RoadSegmentLaneDirection ReadJson(JsonReader reader, Type objectType, RoadSegmentLaneDirection existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return RoadSegmentLaneDirection.Parse(reader.Value?.ToString());
    }

    public override void WriteJson(JsonWriter writer, RoadSegmentLaneDirection value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
