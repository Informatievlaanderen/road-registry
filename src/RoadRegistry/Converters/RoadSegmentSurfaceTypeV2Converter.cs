namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class RoadSegmentSurfaceTypeV2Converter : NullableValueTypeJsonConverter<RoadSegmentSurfaceTypeV2>
{
    protected override RoadSegmentSurfaceTypeV2 ReadJson(object value, Type objectType, JsonSerializer serializer)
    {
        return RoadSegmentSurfaceTypeV2.Parse(value.ToString());
    }

    protected override void WriteJson(JsonWriter writer, RoadSegmentSurfaceTypeV2 value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
