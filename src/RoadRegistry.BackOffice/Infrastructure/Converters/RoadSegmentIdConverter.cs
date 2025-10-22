namespace RoadRegistry.BackOffice.Infrastructure.Converters;

using System;
using Newtonsoft.Json;
using RoadSegment.ValueObjects;

public class RoadSegmentIdConverter : JsonConverter<RoadSegmentId>
{
    public override RoadSegmentId ReadJson(JsonReader reader, Type objectType, RoadSegmentId existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return new RoadSegmentId(int.Parse(reader.Value?.ToString() ?? "0"));
    }

    public override void WriteJson(JsonWriter writer, RoadSegmentId value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToInt32());
    }
}
