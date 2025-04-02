namespace RoadRegistry.BackOffice.ShapeFile;

using Be.Vlaanderen.Basisregisters.Shaperon;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using NetTopologySuite.IO.Esri.Shapefiles.Readers;
using NetTopologySuite.IO.Esri.Shp;
using NetTopologySuite.IO.Streams;

public interface IZipArchiveShapeFileReader
{
    IEnumerable<(Geometry, RecordNumber)> Read(ZipArchiveEntry entry);
}

//TODO-pr versioning?
public class ZipArchiveShapeFileReaderV2: IZipArchiveShapeFileReader
{
    public IEnumerable<(Geometry, RecordNumber)> Read(ZipArchiveEntry entry)
    {
        var recordNumber = RecordNumber.Initial;

        using var fileStream = entry.Open();
        using var memoryStream = new MemoryStream();

        fileStream.CopyTo(memoryStream);
        memoryStream.Position = 0;

        var geometries = Shp.OpenRead(memoryStream, new ShapefileReaderOptions
        {
            Factory = WellKnownGeometryFactories.Default
        }).ToArray();

        foreach (var geometry in geometries)
        {
            yield return (geometry, recordNumber);
            recordNumber = recordNumber.Next();
        }
    }
}

public class ZipArchiveShapeFileReaderV1: IZipArchiveShapeFileReader
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

        foreach (var geometry in shpReader.ReadAllShapes(WellKnownGeometryFactories.Default))
        {
            yield return (geometry, recordNumber);
            recordNumber = recordNumber.Next();
        }
    }
}
