namespace RoadRegistry.BackOffice.Infrastructure.Converters;

using System;
using Newtonsoft.Json;

public class RoadSegmentLaneCountConverter : JsonConverter<RoadSegmentLaneCount>
{
    public override RoadSegmentLaneCount ReadJson(JsonReader reader, Type objectType, RoadSegmentLaneCount existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return new RoadSegmentLaneCount(int.Parse(reader.Value?.ToString() ?? "0"));
    }

    public override void WriteJson(JsonWriter writer, RoadSegmentLaneCount value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
