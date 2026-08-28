namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;
using RoadRegistry.RoadSegment.ValueObjects;

public class RoadSegmentStatusChangeConverter : NullableValueTypeJsonConverter<RoadSegmentStatusChange>
{
    protected override RoadSegmentStatusChange ReadJson(object value, Type objectType, JsonSerializer serializer)
    {
        return RoadSegmentStatusChange.Parse(value.ToString());
    }

    protected override void WriteJson(JsonWriter writer, RoadSegmentStatusChange value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
