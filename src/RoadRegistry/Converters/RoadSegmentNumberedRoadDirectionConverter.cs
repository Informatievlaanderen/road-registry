namespace RoadRegistry.BackOffice.Infrastructure.Converters;

using System;
using Newtonsoft.Json;

public class RoadSegmentNumberedRoadDirectionConverter : JsonConverter<RoadSegmentNumberedRoadDirection>
{
    public override RoadSegmentNumberedRoadDirection ReadJson(JsonReader reader, Type objectType, RoadSegmentNumberedRoadDirection existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return RoadSegmentNumberedRoadDirection.Parse(reader.Value?.ToString());
    }

    public override void WriteJson(JsonWriter writer, RoadSegmentNumberedRoadDirection value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
