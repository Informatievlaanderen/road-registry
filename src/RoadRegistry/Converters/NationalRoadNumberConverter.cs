namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class NationalRoadNumberConverter : NullableJsonConverter<NationalRoadNumber>
{
    public override NationalRoadNumber ReadJson(JsonReader reader, Type objectType, NationalRoadNumber? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return NationalRoadNumber.Parse((string)reader.Value!);
    }

    public override void WriteJson(JsonWriter writer, NationalRoadNumber value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
