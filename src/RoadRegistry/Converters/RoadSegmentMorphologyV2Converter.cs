namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class RoadSegmentMorphologyV2Converter : JsonConverter<RoadSegmentMorphologyV2>
{
    public override RoadSegmentMorphologyV2 ReadJson(JsonReader reader, Type objectType, RoadSegmentMorphologyV2 existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return RoadSegmentMorphologyV2.Parse(reader.Value?.ToString());
    }

    public override void WriteJson(JsonWriter writer, RoadSegmentMorphologyV2 value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
