namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class NumberedRoadNumberConverter : NullableJsonConverter<NumberedRoadNumber>
{
    public override NumberedRoadNumber ReadJson(JsonReader reader, Type objectType, NumberedRoadNumber? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return NumberedRoadNumber.Parse((string)reader.Value!);
    }

    public override void WriteJson(JsonWriter writer, NumberedRoadNumber value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
