namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class RoadSegmentStatusConverter : NullableValueTypeJsonConverter<RoadSegmentStatus>
{
    protected override RoadSegmentStatus ReadJson(object value, Type objectType, JsonSerializer serializer)
    {
        return RoadSegmentStatus.Parse(value.ToString());
    }

    protected override void WriteJson(JsonWriter writer, RoadSegmentStatus value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
