namespace RoadRegistry.BackOffice.Infrastructure.Converters;

using System;
using Newtonsoft.Json;

public class RoadSegmentSurfaceTypeConverter : JsonConverter<RoadSegmentSurfaceType>
{
    public override RoadSegmentSurfaceType ReadJson(JsonReader reader, Type objectType, RoadSegmentSurfaceType existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return RoadSegmentSurfaceType.Parse(reader.Value?.ToString());
    }

    public override void WriteJson(JsonWriter writer, RoadSegmentSurfaceType value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
