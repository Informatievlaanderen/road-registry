namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class RoadSegmentGeometryDrawMethodV2Converter : NullableValueTypeJsonConverter<RoadSegmentGeometryDrawMethodV2>
{
    protected override RoadSegmentGeometryDrawMethodV2 ReadJson(object value, Type objectType, JsonSerializer serializer)
    {
        return RoadSegmentGeometryDrawMethodV2.Parse((string)value);
    }

    protected override void WriteJson(JsonWriter writer, RoadSegmentGeometryDrawMethodV2 value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
