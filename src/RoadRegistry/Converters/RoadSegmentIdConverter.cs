namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;
using RoadRegistry.RoadSegment.ValueObjects;

public class RoadSegmentIdConverter : NullableJsonConverter<RoadSegmentId>
{
    public override RoadSegmentId ReadJson(JsonReader reader, Type objectType, RoadSegmentId? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return new RoadSegmentId(Convert.ToInt32(reader.Value));
    }

    public override void WriteJson(JsonWriter writer, RoadSegmentId value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToInt32());
    }
}
