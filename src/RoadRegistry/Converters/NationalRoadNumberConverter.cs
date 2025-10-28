namespace RoadRegistry.BackOffice.Infrastructure.Converters;

using System;
using Newtonsoft.Json;

public class NationalRoadNumberConverter : JsonConverter<NationalRoadNumber>
{
    public override NationalRoadNumber ReadJson(JsonReader reader, Type objectType, NationalRoadNumber existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return NationalRoadNumber.Parse(reader.Value?.ToString());
    }

    public override void WriteJson(JsonWriter writer, NationalRoadNumber value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
