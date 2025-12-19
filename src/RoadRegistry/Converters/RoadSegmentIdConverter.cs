namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;
using RoadRegistry.RoadSegment.ValueObjects;

public class RoadSegmentIdConverter : NullableValueTypeJsonConverter<RoadSegmentId>
{
    protected override RoadSegmentId ReadJson(object value, Type objectType, JsonSerializer serializer)
    {
        return new RoadSegmentId(Convert.ToInt32(value));
    }

    protected override void WriteJson(JsonWriter writer, RoadSegmentId value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToInt32());
    }
}
