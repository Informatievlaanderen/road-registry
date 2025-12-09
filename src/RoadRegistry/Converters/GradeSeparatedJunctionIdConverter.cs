namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class GradeSeparatedJunctionIdConverter : NullableValueTypeJsonConverter<GradeSeparatedJunctionId>
{
    protected override GradeSeparatedJunctionId ReadJson(object value, Type objectType, JsonSerializer serializer)
    {
        return new GradeSeparatedJunctionId(Convert.ToInt32(value));
    }

    protected override void WriteJson(JsonWriter writer, GradeSeparatedJunctionId value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToInt32());
    }
}
