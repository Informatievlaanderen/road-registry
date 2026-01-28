namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class RoadSegmentSurfaceTypeV2Converter : JsonConverter<RoadSegmentSurfaceTypeV2>
{
    public override RoadSegmentSurfaceTypeV2 ReadJson(JsonReader reader, Type objectType, RoadSegmentSurfaceTypeV2 existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return RoadSegmentSurfaceTypeV2.Parse(reader.Value?.ToString());
    }

    public override void WriteJson(JsonWriter writer, RoadSegmentSurfaceTypeV2 value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
