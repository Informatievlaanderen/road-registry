namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class EuropeanRoadNumberConverter : JsonConverter<EuropeanRoadNumber>
{
    public override EuropeanRoadNumber ReadJson(JsonReader reader, Type objectType, EuropeanRoadNumber existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return EuropeanRoadNumber.Parse(reader.Value?.ToString());
    }

    public override void WriteJson(JsonWriter writer, EuropeanRoadNumber value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
