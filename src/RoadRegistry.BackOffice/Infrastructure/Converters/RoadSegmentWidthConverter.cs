namespace RoadRegistry.BackOffice.Infrastructure.Converters;

using System;
using Newtonsoft.Json;

public class RoadSegmentWidthConverter : JsonConverter<RoadSegmentWidth>
{
    public override RoadSegmentWidth ReadJson(JsonReader reader, Type objectType, RoadSegmentWidth existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return new RoadSegmentWidth(int.Parse(reader.Value?.ToString() ?? "0"));
    }

    public override void WriteJson(JsonWriter writer, RoadSegmentWidth value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
