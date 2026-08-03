namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class RoadSegmentAccessRestrictionV2Converter : NullableValueTypeJsonConverter<RoadSegmentAccessRestrictionV2>
{
    protected override RoadSegmentAccessRestrictionV2 ReadJson(object value, Type objectType, JsonSerializer serializer)
    {
        return RoadSegmentAccessRestrictionV2.Parse(value.ToString());
    }

    protected override void WriteJson(JsonWriter writer, RoadSegmentAccessRestrictionV2 value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
