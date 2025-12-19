namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public abstract class NullableValueTypeJsonConverter<T> : JsonConverter
    where T : notnull
{
    public sealed override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
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

    public sealed override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var value = reader.Value;
        if (value is null)
        {
            return null;
        }

        return ReadJson(value, objectType, serializer);
    }

    protected abstract void WriteJson(JsonWriter writer, T value, JsonSerializer serializer);
    protected abstract T ReadJson(object value, Type objectType, JsonSerializer serializer);

    public override bool CanConvert(Type objectType)
    {
        return (Nullable.GetUnderlyingType(objectType) ?? objectType) == typeof(T);
    }
}
