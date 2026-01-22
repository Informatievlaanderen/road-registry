namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class RoadSegmentAccessRestrictionV2Converter : JsonConverter<RoadSegmentAccessRestrictionV2>
{
    public override RoadSegmentAccessRestrictionV2 ReadJson(JsonReader reader, Type objectType, RoadSegmentAccessRestrictionV2 existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return RoadSegmentAccessRestrictionV2.Parse(reader.Value?.ToString());
    }

    public override void WriteJson(JsonWriter writer, RoadSegmentAccessRestrictionV2 value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
