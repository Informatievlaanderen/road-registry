namespace RoadRegistry.BackOffice.Infrastructure.Converters;

using System;
using Newtonsoft.Json;

public class RoadSegmentPositionConverter : JsonConverter<RoadSegmentPosition>
{
    public override RoadSegmentPosition ReadJson(JsonReader reader, Type objectType, RoadSegmentPosition existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.Value is null)
        {
            return RoadSegmentPosition.Zero;
        }
        
        return RoadSegmentPosition.FromDouble(Convert.ToDouble(reader.Value));
    }

    public override void WriteJson(JsonWriter writer, RoadSegmentPosition value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToDecimal());
    }
}
