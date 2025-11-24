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


public class NullableRoadNodeIdConverter : JsonConverter<RoadNodeId?>
{
    public override RoadNodeId? ReadJson(JsonReader reader, Type objectType, RoadNodeId? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.Value is null)
        {
            return null;
        }

        return new RoadNodeId(int.Parse(reader.Value.ToString() ?? "0"));
    }

    public override void WriteJson(JsonWriter writer, RoadNodeId? value, JsonSerializer serializer)
    {
        if (value is not null)
        {
            writer.WriteValue(value.Value.ToInt32());
        }
        else
        {
            writer.WriteNull();
        }
    }
}
