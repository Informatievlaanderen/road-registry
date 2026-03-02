namespace RoadRegistry.Converters;

using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Newtonsoft.Json;

public sealed class NtsGeometryJsonConverter : JsonConverter<Geometry>
{
    private static readonly WKTReader WktReader = new();
    private static readonly WKTWriter WktWriter = new();

    public override void WriteJson(JsonWriter writer, Geometry? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteStartObject();

        writer.WritePropertyName("srid");
        writer.WriteValue(value.SRID);

        writer.WritePropertyName("wkt");
        writer.WriteValue(WktWriter.Write(value));

        writer.WriteEndObject();
    }

    public override Geometry? ReadJson(
        JsonReader reader,
        Type objectType,
        Geometry? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        var srid = 0;
        string? wkt = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonToken.EndObject)
                break;

            if (reader.TokenType != JsonToken.PropertyName)
                continue;

            var propertyName = (string)reader.Value!;
            reader.Read();

            switch (propertyName)
            {
                case "srid":
                    srid = Convert.ToInt32(reader.Value);
                    break;

                case "wkt":
                    wkt = reader.Value?.ToString();
                    break;
            }
        }

        if (string.IsNullOrWhiteSpace(wkt))
            return null;

        var geometry = WktReader.Read(wkt);
        geometry.SRID = srid;

        return geometry;
    }
}
