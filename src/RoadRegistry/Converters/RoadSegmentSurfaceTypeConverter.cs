namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class RoadSegmentSurfaceTypeConverter : NullableValueTypeJsonConverter<RoadSegmentSurfaceType>
{
    protected override RoadSegmentSurfaceType ReadJson(object value, Type objectType, JsonSerializer serializer)
    {
        return RoadSegmentSurfaceType.Parse(value.ToString());
    }

    protected override void WriteJson(JsonWriter writer, RoadSegmentSurfaceType value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
