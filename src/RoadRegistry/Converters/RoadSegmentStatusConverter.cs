namespace RoadRegistry.BackOffice.Infrastructure.Converters;

using System;
using Newtonsoft.Json;

public class RoadSegmentStatusConverter : JsonConverter<RoadSegmentStatus>
{
    public override RoadSegmentStatus ReadJson(JsonReader reader, Type objectType, RoadSegmentStatus existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return RoadSegmentStatus.Parse(reader.Value?.ToString());
    }

    public override void WriteJson(JsonWriter writer, RoadSegmentStatus value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
