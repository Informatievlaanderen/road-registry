namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class RoadNodeTypeConverter : NullableValueTypeJsonConverter<RoadNodeType>
{
    protected override RoadNodeType ReadJson(object value, Type objectType, JsonSerializer serializer)
    {
        return RoadNodeType.Parse((string)value);
    }

    protected override void WriteJson(JsonWriter writer, RoadNodeType value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
