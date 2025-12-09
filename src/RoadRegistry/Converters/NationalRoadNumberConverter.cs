namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class NationalRoadNumberConverter : NullableValueTypeJsonConverter<NationalRoadNumber>
{
    protected override NationalRoadNumber ReadJson(object value, Type objectType, JsonSerializer serializer)
    {
        return NationalRoadNumber.Parse((string)value);
    }

    protected override void WriteJson(JsonWriter writer, NationalRoadNumber value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
