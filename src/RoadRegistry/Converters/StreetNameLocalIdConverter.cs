namespace RoadRegistry.BackOffice.Infrastructure.Converters
{
    using System;
    using Newtonsoft.Json;

    public class StreetNameLocalIdConverter : NullableJsonConverter<StreetNameLocalId>
    {
        public override StreetNameLocalId ReadJson(JsonReader reader, Type objectType, StreetNameLocalId? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return new StreetNameLocalId(Convert.ToInt32(reader.Value));
        }

        public override void WriteJson(JsonWriter writer, StreetNameLocalId value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToInt32());
        }
    }
}
