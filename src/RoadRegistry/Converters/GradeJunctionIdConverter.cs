namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class GradeJunctionIdConverter : NullableValueTypeJsonConverter<GradeJunctionId>
{
    protected override GradeJunctionId ReadJson(object value, Type objectType, JsonSerializer serializer)
    {
        return new GradeJunctionId(Convert.ToInt32(value));
    }

    protected override void WriteJson(JsonWriter writer, GradeJunctionId value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToInt32());
    }
}
