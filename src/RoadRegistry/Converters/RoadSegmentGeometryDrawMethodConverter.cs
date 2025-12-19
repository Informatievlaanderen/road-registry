namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class RoadSegmentGeometryDrawMethodConverter : NullableValueTypeJsonConverter<RoadSegmentGeometryDrawMethod>
{
    protected override RoadSegmentGeometryDrawMethod ReadJson(object value, Type objectType, JsonSerializer serializer)
    {
        return RoadSegmentGeometryDrawMethod.Parse((string)value);
    }

    protected override void WriteJson(JsonWriter writer, RoadSegmentGeometryDrawMethod value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
