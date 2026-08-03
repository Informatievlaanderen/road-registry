namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class RoadSegmentMorphologyV2Converter : NullableValueTypeJsonConverter<RoadSegmentMorphologyV2>
{
    protected override RoadSegmentMorphologyV2 ReadJson(object value, Type objectType, JsonSerializer serializer)
    {
        return RoadSegmentMorphologyV2.Parse(value.ToString());
    }

    protected override void WriteJson(JsonWriter writer, RoadSegmentMorphologyV2 value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
