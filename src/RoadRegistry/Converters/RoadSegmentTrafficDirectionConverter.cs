namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;
using RoadRegistry.ValueObjects;

public class RoadSegmentTrafficDirectionConverter : NullableValueTypeJsonConverter<RoadSegmentTrafficDirection>
{
    protected override RoadSegmentTrafficDirection ReadJson(object value, Type objectType, JsonSerializer serializer)
    {
        return RoadSegmentTrafficDirection.Parse(value.ToString());
    }

    protected override void WriteJson(JsonWriter writer, RoadSegmentTrafficDirection value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
