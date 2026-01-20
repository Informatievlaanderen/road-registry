namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class RoadNodeTypeV2Converter : NullableValueTypeJsonConverter<RoadNodeTypeV2>
{
    protected override RoadNodeTypeV2 ReadJson(object value, Type objectType, JsonSerializer serializer)
    {
        return RoadNodeTypeV2.Parse((string)value);
    }

    protected override void WriteJson(JsonWriter writer, RoadNodeTypeV2 value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
