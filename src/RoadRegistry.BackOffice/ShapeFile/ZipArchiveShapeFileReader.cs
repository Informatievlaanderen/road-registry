namespace RoadRegistry.BackOffice.ShapeFile;

using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Streams;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

public class ZipArchiveShapeFileReader
{
    public IEnumerable<(Geometry, RecordNumber)> Read(ZipArchiveEntry entry)
    {
        var recordNumber = RecordNumber.Initial;

        using var fileStream = entry.Open();
        using var memoryStream = new MemoryStream();

        fileStream.CopyTo(memoryStream);
        memoryStream.Position = 0;

        var streamProvider = new ExternallyManagedStreamProvider(StreamTypes.Shape, memoryStream);
        var streamProviderRegistry = new ShapefileStreamProviderRegistry(streamProvider, null);
        var shpReader = new NetTopologySuite.IO.ShapeFile.Extended.ShapeReader(streamProviderRegistry);

        foreach (var geometry in shpReader.ReadAllShapes(GeometryConfiguration.GeometryFactory))
        {
            yield return (geometry, recordNumber);
            recordNumber = recordNumber.Next();
        }
    }
}
