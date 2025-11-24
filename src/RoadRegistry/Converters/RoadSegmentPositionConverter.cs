namespace RoadRegistry.BackOffice.Infrastructure.Converters;

using System;
using Newtonsoft.Json;

public class RoadSegmentPositionConverter : NullableJsonConverter<RoadSegmentPosition>
{
    public override RoadSegmentPosition ReadJson(JsonReader reader, Type objectType, RoadSegmentPosition? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return RoadSegmentPosition.FromDouble(Convert.ToDouble(reader.Value));
    }

    public override void WriteJson(JsonWriter writer, RoadSegmentPosition value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToDecimal());
    }
}
