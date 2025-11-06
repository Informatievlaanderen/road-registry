namespace RoadRegistry.BackOffice.Infrastructure.Converters;

using System;
using Newtonsoft.Json;
using RoadSegment.ValueObjects;

public class RoadSegmentIdConverter : JsonConverter<RoadSegmentId?>
{
    public override RoadSegmentId? ReadJson(JsonReader reader, Type objectType, RoadSegmentId? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.Value is null)
        {
            return null;
        }

        return new RoadSegmentId(int.Parse(reader.Value.ToString() ?? "0"));
    }

    public override void WriteJson(JsonWriter writer, RoadSegmentId? value, JsonSerializer serializer)
    {
        if (value is null)
        {
            writer.WriteNull();
        }
        else
        {
            writer.WriteValue(value.Value.ToInt32());
        }
    }
}
