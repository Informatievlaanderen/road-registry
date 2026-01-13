namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class UploadIdConverter : NullableValueTypeJsonConverter<UploadId>
{
    protected override UploadId ReadJson(object value, Type objectType, JsonSerializer serializer)
    {
        return new UploadId(Guid.Parse((string)value));
    }

    protected override void WriteJson(JsonWriter writer, UploadId value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToGuid());
    }
}
