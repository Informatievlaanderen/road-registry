namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class RoadSegmentNumberedRoadDirectionConverter : NullableValueTypeJsonConverter<RoadSegmentNumberedRoadDirection>
{
    protected override RoadSegmentNumberedRoadDirection ReadJson(object value, Type objectType, JsonSerializer serializer)
    {
        return RoadSegmentNumberedRoadDirection.Parse(value.ToString());
    }

    protected override void WriteJson(JsonWriter writer, RoadSegmentNumberedRoadDirection value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
