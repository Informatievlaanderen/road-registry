namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class RoadSegmentLaneDirectionConverter : NullableValueTypeJsonConverter<RoadSegmentLaneDirection>
{
    protected override RoadSegmentLaneDirection ReadJson(object value, Type objectType, JsonSerializer serializer)
    {
        return RoadSegmentLaneDirection.Parse(value.ToString());
    }

    protected override void WriteJson(JsonWriter writer, RoadSegmentLaneDirection value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
