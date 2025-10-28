namespace RoadRegistry.BackOffice.Infrastructure.Converters;

using System;
using Newtonsoft.Json;
using RoadSegment.ValueObjects;

public class RoadNodeIdConverter : JsonConverter<RoadNodeId>
{
    public override RoadNodeId ReadJson(JsonReader reader, Type objectType, RoadNodeId existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return new RoadNodeId(int.Parse(reader.Value?.ToString() ?? "0"));
    }

    public override void WriteJson(JsonWriter writer, RoadNodeId value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToInt32());
    }
}
