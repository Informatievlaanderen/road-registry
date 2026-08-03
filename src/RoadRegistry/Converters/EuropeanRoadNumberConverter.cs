namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class EuropeanRoadNumberConverter : NullableValueTypeJsonConverter<EuropeanRoadNumber>
{
    protected override EuropeanRoadNumber ReadJson(object value, Type objectType, JsonSerializer serializer)
    {
        return EuropeanRoadNumber.Parse(value.ToString());
    }

    protected override void WriteJson(JsonWriter writer, EuropeanRoadNumber value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
