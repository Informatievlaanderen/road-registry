namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;
using ScopedRoadNetwork.ValueObjects;

public class RoadNetworkIdConverter : NullableValueTypeJsonConverter<ScopedRoadNetworkId>
{
    protected override ScopedRoadNetworkId ReadJson(object value, Type objectType, JsonSerializer serializer)
    {
        return new ScopedRoadNetworkId((string)value);
    }

    protected override void WriteJson(JsonWriter writer, ScopedRoadNetworkId value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
