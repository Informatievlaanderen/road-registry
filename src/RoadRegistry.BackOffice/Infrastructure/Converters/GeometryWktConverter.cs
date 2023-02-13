namespace RoadRegistry.BackOffice.Infrastructure.Converters;

using System;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Newtonsoft.Json;

public class GeometryWktConverterConverter<TGeometry> : JsonConverter<TGeometry>
    where TGeometry : Geometry
{
    public override TGeometry ReadJson(JsonReader reader, Type objectType, TGeometry existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var wktReader = new WKTReader(new NtsGeometryServices(
            GeometryConfiguration.GeometryFactory.PrecisionModel,
            GeometryConfiguration.GeometryFactory.SRID
        ));

        return (TGeometry)wktReader.Read(reader.Value?.ToString());
    }

    public override void WriteJson(JsonWriter writer, TGeometry value, JsonSerializer serializer)
    {
        writer.WriteValue(value.AsText());
    }
}

public class MultiLineStringWktConverter : GeometryWktConverterConverter<MultiLineString>
{
}
