namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;
using RoadRegistry.RoadSegment.ValueObjects;

public class RoadSegmentAttributeSideConverter : JsonConverter<RoadSegmentAttributeSide>
{
    public override RoadSegmentAttributeSide? ReadJson(JsonReader reader, Type objectType, RoadSegmentAttributeSide? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var valueAsString = reader.Value?.ToString();
        if (valueAsString is null)
        {
            return RoadSegmentAttributeSide.Beide;
        }

        if (RoadSegmentAttributeSide.TryParse(valueAsString, out var sideFromString))
        {
            return sideFromString;
        }

        if (int.TryParse(valueAsString, out var identifier) && RoadSegmentAttributeSide.ByIdentifier.TryGetValue(identifier, out var sideFromInt))
        {
            return sideFromInt;
        }

        throw new FormatException($"The value {valueAsString} is not a well known road segment attribute side.");
    }

    public override void WriteJson(JsonWriter writer, RoadSegmentAttributeSide? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
        }
        else
        {
            writer.WriteValue(value.ToString());
        }
    }
}
