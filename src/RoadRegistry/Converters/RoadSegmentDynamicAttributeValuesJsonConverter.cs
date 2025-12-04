namespace RoadRegistry.Converters;

using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using RoadSegment.ValueObjects;

public class RoadSegmentDynamicAttributeValuesJsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType.IsGenericType
               && objectType.GetGenericTypeDefinition() == typeof(RoadSegmentDynamicAttributeValues<>);
    }

    public sealed override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (value is null)
        {
            writer.WriteNull();
        }
        else
        {
            var valuesProp = value.GetType().GetProperty(nameof(RoadSegmentDynamicAttributeValues<object>.Values));
            var values = valuesProp?.GetValue(value);

            serializer.Serialize(writer, values);
        }
    }

    public sealed override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        var tArg = objectType.GetGenericArguments()[0];
        var itemType = typeof(RoadSegmentDynamicAttributeValue<>).MakeGenericType(tArg);
        var listType = typeof(List<>).MakeGenericType(itemType);

        var list = serializer.Deserialize(reader, listType);

        var instance = Activator.CreateInstance(objectType, list);
        return instance!;
    }
}
