namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;
using RoadRegistry.ValueObjects;

public class RoadSegmentPedestrianTrafficDirectionConverter : JsonConverter<RoadSegmentPedestrianTrafficDirection>
{
    public override RoadSegmentPedestrianTrafficDirection ReadJson(JsonReader reader, Type objectType, RoadSegmentPedestrianTrafficDirection existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return RoadSegmentPedestrianTrafficDirection.Parse(reader.Value?.ToString());
    }

    public override void WriteJson(JsonWriter writer, RoadSegmentPedestrianTrafficDirection value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
