namespace RoadRegistry.BackOffice.Infrastructure.Converters;

using System;
using Newtonsoft.Json;
using RoadSegment.ValueObjects;

public class GradeSeparatedJunctionIdConverter : JsonConverter<GradeSeparatedJunctionId>
{
    public override GradeSeparatedJunctionId ReadJson(JsonReader reader, Type objectType, GradeSeparatedJunctionId existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return new GradeSeparatedJunctionId(int.Parse(reader.Value?.ToString() ?? "0"));
    }

    public override void WriteJson(JsonWriter writer, GradeSeparatedJunctionId value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToInt32());
    }
}
