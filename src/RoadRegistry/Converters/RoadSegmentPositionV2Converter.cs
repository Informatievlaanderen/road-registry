namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class RoadSegmentPositionV2Converter : NullableValueTypeJsonConverter<RoadSegmentPositionV2>
{
    protected override RoadSegmentPositionV2 ReadJson(object value, Type objectType, JsonSerializer serializer)
    {
        return new RoadSegmentPositionV2(Convert.ToDouble(value));
    }

    protected override void WriteJson(JsonWriter writer, RoadSegmentPositionV2 value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToDouble());
    }
}
