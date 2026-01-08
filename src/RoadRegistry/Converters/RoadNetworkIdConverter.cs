namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;
using RoadNetwork.ValueObjects;

public class RoadNetworkIdConverter : NullableValueTypeJsonConverter<RoadNetworkId>
{
    protected override RoadNetworkId ReadJson(object value, Type objectType, JsonSerializer serializer)
    {
        return new RoadNetworkId((string)value);
    }

    protected override void WriteJson(JsonWriter writer, RoadNetworkId value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
