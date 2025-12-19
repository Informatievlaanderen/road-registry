namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class RoadSegmentPositionConverter : NullableValueTypeJsonConverter<RoadSegmentPosition>
{
    protected override RoadSegmentPosition ReadJson(object value, Type objectType, JsonSerializer serializer)
    {
        return RoadSegmentPosition.FromDouble(Convert.ToDouble(value));
    }

    protected override void WriteJson(JsonWriter writer, RoadSegmentPosition value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToDecimal());
    }
}
