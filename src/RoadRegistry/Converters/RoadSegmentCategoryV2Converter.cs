namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class RoadSegmentCategoryV2Converter : JsonConverter<RoadSegmentCategoryV2>
{
    public override RoadSegmentCategoryV2 ReadJson(JsonReader reader, Type objectType, RoadSegmentCategoryV2 existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return RoadSegmentCategoryV2.Parse(reader.Value?.ToString());
    }

    public override void WriteJson(JsonWriter writer, RoadSegmentCategoryV2 value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
