namespace RoadRegistry.BackOffice.Infrastructure.Converters;

using System;
using Newtonsoft.Json;

public class RoadSegmentCategoryConverter : JsonConverter<RoadSegmentCategory>
{
    public override RoadSegmentCategory ReadJson(JsonReader reader, Type objectType, RoadSegmentCategory existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return RoadSegmentCategory.Parse(reader.Value?.ToString());
    }

    public override void WriteJson(JsonWriter writer, RoadSegmentCategory value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
