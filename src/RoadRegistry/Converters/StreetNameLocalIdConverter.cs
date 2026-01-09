namespace RoadRegistry.Converters
{
    using System;
    using Newtonsoft.Json;

    public class StreetNameLocalIdConverter : NullableValueTypeJsonConverter<StreetNameLocalId>
    {
        protected override StreetNameLocalId ReadJson(object value, Type objectType, JsonSerializer serializer)
        {
            return new StreetNameLocalId(Convert.ToInt32(value));
        }

        protected override void WriteJson(JsonWriter writer, StreetNameLocalId value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToInt32());
        }
    }
}
