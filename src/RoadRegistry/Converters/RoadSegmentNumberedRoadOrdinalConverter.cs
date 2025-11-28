namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class RoadSegmentNumberedRoadOrdinalConverter : JsonConverter<RoadSegmentNumberedRoadOrdinal>
{
    public override RoadSegmentNumberedRoadOrdinal ReadJson(JsonReader reader, Type objectType, RoadSegmentNumberedRoadOrdinal existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return new RoadSegmentNumberedRoadOrdinal(int.Parse(reader.Value?.ToString() ?? "0"));
    }

    public override void WriteJson(JsonWriter writer, RoadSegmentNumberedRoadOrdinal value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
