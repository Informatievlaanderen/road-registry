namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class RoadSegmentAccessRestrictionConverter : NullableValueTypeJsonConverter<RoadSegmentAccessRestriction>
{
    protected override RoadSegmentAccessRestriction ReadJson(object value, Type objectType, JsonSerializer serializer)
    {
        return RoadSegmentAccessRestriction.Parse(value.ToString());
    }

    protected override void WriteJson(JsonWriter writer, RoadSegmentAccessRestriction value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
