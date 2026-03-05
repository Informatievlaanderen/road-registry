namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class RoadSegmentTempIdConverter : NullableValueTypeJsonConverter<RoadSegmentTempId>
{
    protected override RoadSegmentTempId ReadJson(object value, Type objectType, JsonSerializer serializer)
    {
        return new RoadSegmentTempId(Convert.ToInt32(value));
    }

    protected override void WriteJson(JsonWriter writer, RoadSegmentTempId value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToInt32());
    }
}
