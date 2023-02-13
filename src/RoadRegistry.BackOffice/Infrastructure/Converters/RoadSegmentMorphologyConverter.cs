namespace RoadRegistry.BackOffice.Infrastructure.Converters;

using System;
using Newtonsoft.Json;

public class RoadSegmentMorphologyConverter : JsonConverter<RoadSegmentMorphology>
{
    public override RoadSegmentMorphology ReadJson(JsonReader reader, Type objectType, RoadSegmentMorphology existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return RoadSegmentMorphology.Parse(reader.Value?.ToString());
    }

    public override void WriteJson(JsonWriter writer, RoadSegmentMorphology value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
