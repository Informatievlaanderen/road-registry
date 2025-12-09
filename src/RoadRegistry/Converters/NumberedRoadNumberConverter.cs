namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class NumberedRoadNumberConverter : NullableValueTypeJsonConverter<NumberedRoadNumber>
{
    protected override NumberedRoadNumber ReadJson(object value, Type objectType, JsonSerializer serializer)
    {
        return NumberedRoadNumber.Parse((string)value);
    }

    protected override void WriteJson(JsonWriter writer, NumberedRoadNumber value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
