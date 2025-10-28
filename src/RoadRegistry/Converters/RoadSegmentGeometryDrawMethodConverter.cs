namespace RoadRegistry.BackOffice.Infrastructure.Converters;

using System;
using Newtonsoft.Json;

public class RoadSegmentGeometryDrawMethodConverter : JsonConverter<RoadSegmentGeometryDrawMethod>
{
    public override RoadSegmentGeometryDrawMethod ReadJson(JsonReader reader, Type objectType, RoadSegmentGeometryDrawMethod existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return RoadSegmentGeometryDrawMethod.Parse(reader.Value?.ToString());
    }

    public override void WriteJson(JsonWriter writer, RoadSegmentGeometryDrawMethod value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
