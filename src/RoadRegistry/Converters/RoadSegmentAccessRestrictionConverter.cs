namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class RoadSegmentAccessRestrictionConverter : JsonConverter<RoadSegmentAccessRestriction>
{
    public override RoadSegmentAccessRestriction ReadJson(JsonReader reader, Type objectType, RoadSegmentAccessRestriction existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return RoadSegmentAccessRestriction.Parse(reader.Value?.ToString());
    }

    public override void WriteJson(JsonWriter writer, RoadSegmentAccessRestriction value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
