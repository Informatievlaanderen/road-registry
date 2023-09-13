namespace RoadRegistry.BackOffice.ShapeFile;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.IO;
using NetTopologySuite.IO.Streams;

public class ZipArchiveShapeFileWriter
{
    private readonly Encoding _encoding;

    public ZipArchiveShapeFileWriter(Encoding encoding)
    {
        _encoding = encoding.ThrowIfNull();
    }

    public void Write(Stream shpStream, Stream shxStream, IEnumerable<IFeature> features)
    {
        ArgumentNullException.ThrowIfNull(shpStream);
        ArgumentNullException.ThrowIfNull(shxStream);
        ArgumentNullException.ThrowIfNull(features);

        var featuresList = features.ToList();

        using var dbfStream = new MemoryStream();

        var shpStreamProvider = new ExternallyManagedStreamProvider(StreamTypes.Shape, shpStream);
        var dbfStreamProvider = new ExternallyManagedStreamProvider(StreamTypes.Data, dbfStream);
        var shxStreamProvider = new ExternallyManagedStreamProvider(StreamTypes.Index, shxStream);
        var streamProviderRegistry = new ShapefileStreamProviderRegistry(shpStreamProvider, dbfStreamProvider, shxStreamProvider);

        var writer = new ShapefileDataWriter(streamProviderRegistry, GeometryConfiguration.GeometryFactory, _encoding);
        var dbaseFileHeader = ShapefileDataWriter.GetHeader(featuresList[0], featuresList.Count);
        writer.Header = dbaseFileHeader;
        writer.Write(featuresList);
    }
}
