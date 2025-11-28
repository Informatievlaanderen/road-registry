namespace RoadRegistry.Converters;

using System;
using Newtonsoft.Json;

public class DownloadIdConverter : NullableJsonConverter<DownloadId>
{
    public override DownloadId ReadJson(JsonReader reader, Type objectType, DownloadId? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return new DownloadId(Guid.Parse((string)reader.Value!));
    }

    public override void WriteJson(JsonWriter writer, DownloadId value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToGuid());
    }
}
