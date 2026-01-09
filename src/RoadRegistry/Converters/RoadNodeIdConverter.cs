namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class RoadNodeIdConverter : NullableValueTypeJsonConverter<RoadNodeId>
{
    protected override RoadNodeId ReadJson(object value, Type objectType, JsonSerializer serializer)
    {
        return new RoadNodeId(Convert.ToInt32(value));
    }

    protected override void WriteJson(JsonWriter writer, RoadNodeId value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToInt32());
    }
}

