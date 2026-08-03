namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class RoadSegmentMorphologyConverter : NullableValueTypeJsonConverter<RoadSegmentMorphology>
{
    protected override RoadSegmentMorphology ReadJson(object value, Type objectType, JsonSerializer serializer)
    {
        return RoadSegmentMorphology.Parse(value.ToString());
    }

    protected override void WriteJson(JsonWriter writer, RoadSegmentMorphology value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
