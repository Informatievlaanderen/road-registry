namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class GradeSeparatedJunctionTypeV2Converter : NullableValueTypeJsonConverter<GradeSeparatedJunctionTypeV2>
{
    protected override GradeSeparatedJunctionTypeV2 ReadJson(object value, Type objectType, JsonSerializer serializer)
    {
        return GradeSeparatedJunctionTypeV2.Parse((string)value);
    }

    protected override void WriteJson(JsonWriter writer, GradeSeparatedJunctionTypeV2 value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
