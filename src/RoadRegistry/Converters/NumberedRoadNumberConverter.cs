namespace RoadRegistry.BackOffice.Infrastructure.Converters;

using System;
using Newtonsoft.Json;

public class NumberedRoadNumberConverter : JsonConverter<NumberedRoadNumber>
{
    public override NumberedRoadNumber ReadJson(JsonReader reader, Type objectType, NumberedRoadNumber existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return NumberedRoadNumber.Parse(reader.Value?.ToString());
    }

    public override void WriteJson(JsonWriter writer, NumberedRoadNumber value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
