namespace RoadRegistry.BackOffice.Infrastructure.Converters
{
    using System;
    using Newtonsoft.Json;

    public class StreetNameLocalIdConverter : JsonConverter<StreetNameLocalId?>
    {
        public override StreetNameLocalId? ReadJson(JsonReader reader, Type objectType, StreetNameLocalId? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value?.ToString() is null)
            {
                return null;
            }

            return new StreetNameLocalId(int.Parse(reader.Value!.ToString()!));
        }

        public override void WriteJson(JsonWriter writer, StreetNameLocalId? value, JsonSerializer serializer)
        {
            if (!value.HasValue) return;

            writer.WriteValue(value.Value.ToInt32());
        }
    }
}
