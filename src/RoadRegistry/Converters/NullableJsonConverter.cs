namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public abstract class NullableJsonConverter<T> : JsonConverter
    where T : struct
{
    public sealed override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
        }
        else
        {
            WriteJson(writer, (T)value, serializer);
        }
    }

    public sealed override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var value = reader.Value;
        if (value is null)
        {
            return null;
        }

        return ReadJson(reader, objectType, (T?)existingValue, existingValue is not null, serializer);
    }

    public abstract void WriteJson(JsonWriter writer, T value, JsonSerializer serializer);
    public abstract T ReadJson(JsonReader reader, Type objectType, T? existingValue, bool hasExistingValue, JsonSerializer serializer);

    public override bool CanConvert(Type objectType)
    {
        return (Nullable.GetUnderlyingType(objectType) ?? objectType) == typeof(T);
    }
}
