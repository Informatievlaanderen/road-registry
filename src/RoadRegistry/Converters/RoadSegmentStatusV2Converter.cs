namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class RoadSegmentStatusV2Converter : NullableValueTypeJsonConverter<RoadSegmentStatusV2>
{
    protected override RoadSegmentStatusV2 ReadJson(object value, Type objectType, JsonSerializer serializer)
    {
        return RoadSegmentStatusV2.Parse(value.ToString());
    }

    protected override void WriteJson(JsonWriter writer, RoadSegmentStatusV2 value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
