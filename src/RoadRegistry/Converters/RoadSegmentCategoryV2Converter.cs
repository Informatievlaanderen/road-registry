namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class RoadSegmentCategoryV2Converter : NullableValueTypeJsonConverter<RoadSegmentCategoryV2>
{
    protected override RoadSegmentCategoryV2 ReadJson(object value, Type objectType, JsonSerializer serializer)
    {
        return RoadSegmentCategoryV2.Parse(value.ToString());
    }

    protected override void WriteJson(JsonWriter writer, RoadSegmentCategoryV2 value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
