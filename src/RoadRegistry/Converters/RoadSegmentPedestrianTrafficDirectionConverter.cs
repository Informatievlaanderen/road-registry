namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;
using RoadRegistry.ValueObjects;

public class RoadSegmentPedestrianTrafficDirectionConverter : NullableValueTypeJsonConverter<RoadSegmentPedestrianTrafficDirection>
{
    protected override RoadSegmentPedestrianTrafficDirection ReadJson(object value, Type objectType, JsonSerializer serializer)
    {
        return RoadSegmentPedestrianTrafficDirection.Parse(value.ToString());
    }

    protected override void WriteJson(JsonWriter writer, RoadSegmentPedestrianTrafficDirection value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
