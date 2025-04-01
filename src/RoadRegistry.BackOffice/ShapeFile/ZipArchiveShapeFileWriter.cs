namespace RoadRegistry.BackOffice.ShapeFile;

using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NetTopologySuite.IO.Esri;

public class ZipArchiveShapeFileWriter
{
    private readonly Encoding _encoding;

    public ZipArchiveShapeFileWriter(Encoding encoding)
    {
        _encoding = encoding.ThrowIfNull();
    }

    public void Write(Stream shpStream, Stream shxStream, IEnumerable<IFeature> features, GeometryFactory geometryFactory)
    {
        ArgumentNullException.ThrowIfNull(shpStream);
        ArgumentNullException.ThrowIfNull(shxStream);
        ArgumentNullException.ThrowIfNull(features);

        var featuresList = features.ToList();

        using var dbfStream = new MemoryStream();

        Shapefile.WriteAllFeatures(
            features: featuresList,
            shpStream: shpStream,
            shxStream: shxStream,
            dbfStream: dbfStream,
            encoding: _encoding);
    }
}
