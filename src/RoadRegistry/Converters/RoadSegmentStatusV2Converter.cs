namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class RoadSegmentStatusV2Converter : JsonConverter<RoadSegmentStatusV2>
{
    public override RoadSegmentStatusV2 ReadJson(JsonReader reader, Type objectType, RoadSegmentStatusV2 existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return RoadSegmentStatusV2.Parse(reader.Value?.ToString());
    }

    public override void WriteJson(JsonWriter writer, RoadSegmentStatusV2 value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
