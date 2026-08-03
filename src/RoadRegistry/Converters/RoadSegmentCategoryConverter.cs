namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class RoadSegmentCategoryConverter : NullableValueTypeJsonConverter<RoadSegmentCategory>
{
    protected override RoadSegmentCategory ReadJson(object value, Type objectType, JsonSerializer serializer)
    {
        return RoadSegmentCategory.Parse(value.ToString());
    }

    protected override void WriteJson(JsonWriter writer, RoadSegmentCategory value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
