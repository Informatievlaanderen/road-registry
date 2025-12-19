namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class ExtractRequestIdConverter : NullableValueTypeJsonConverter<ExtractRequestId>
{
    protected override ExtractRequestId ReadJson(object value, Type objectType, JsonSerializer serializer)
    {
        return ExtractRequestId.FromString((string)value);
    }

    protected override void WriteJson(JsonWriter writer, ExtractRequestId value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
