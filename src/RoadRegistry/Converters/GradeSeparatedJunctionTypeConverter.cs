namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class GradeSeparatedJunctionTypeConverter : NullableValueTypeJsonConverter<GradeSeparatedJunctionType>
{
    protected override GradeSeparatedJunctionType ReadJson(object value, Type objectType, JsonSerializer serializer)
    {
        return GradeSeparatedJunctionType.Parse((string)value);
    }

    protected override void WriteJson(JsonWriter writer, GradeSeparatedJunctionType value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
