namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;
using RoadRegistry.ValueObjects;

public class RoadSegmentTrafficDirectionConverter : JsonConverter<RoadSegmentTrafficDirection>
{
    public override RoadSegmentTrafficDirection ReadJson(JsonReader reader, Type objectType, RoadSegmentTrafficDirection existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return RoadSegmentTrafficDirection.Parse(reader.Value?.ToString());
    }

    public override void WriteJson(JsonWriter writer, RoadSegmentTrafficDirection value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
