namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class DownloadIdConverter : NullableValueTypeJsonConverter<DownloadId>
{
    protected override DownloadId ReadJson(object value, Type objectType, JsonSerializer serializer)
    {
        return new DownloadId(Guid.Parse((string)value));
    }

    protected override void WriteJson(JsonWriter writer, DownloadId value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToGuid());
    }
}
